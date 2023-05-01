using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DapperLabs.Flow.Sdk.Cadence;
using DapperLabs.Flow.Sdk.DataObjects;
using DapperLabs.Flow.Sdk.Exceptions;
using DapperLabs.Flow.Sdk.Network;

namespace DapperLabs.Flow.Sdk
{
	/// <summary>
	/// Functions relating to Flow Transactions
	/// </summary>
    public class Transactions
    {
        // need to find out what this value should be, and allow developers to set it
        private const ulong TransactionGasLimit = 9999;     // acceptable range 1 - 9999
        private static Dictionary<string, uint> SequenceNumberCache = new Dictionary<string, uint>();
        private static HashSet<string> SequenceRecoverySet = new HashSet<string>();
        private static int SequenceNumberRecoveryDelay = 15000;

		private static List<Task> SequenceErrorMonitoringTasks = new List<Task>();

		/// <summary>
		/// Used to reset sequence tracking data.  Used when resetting emulator state because sequence numbers will reset at that point.
		/// </summary>
		public static void ResetSequenceTracking()
		{
			SequenceErrorMonitoringTasks.Clear();
			SequenceRecoverySet.Clear();
			SequenceNumberCache.Clear();
		}
		
		/// <summary>
		/// Submits a transaction to the blockchain
		/// </summary>
		/// <param name="script">The text contents of the transaction to execute</param>
		/// <param name="arguments">Cadence arguments that will be passed to the transaction</param>
		/// <returns>A Task that will resolve to a FlowTransactionResponse upon completion</returns>
        public static async Task<FlowTransactionResponse> Submit(string script, List<CadenceBase> arguments=null)
        {
			try
			{
				arguments = arguments ?? new List<CadenceBase>();
				byte[] signature = null;

				FlowBlock block = await Blocks.GetLatest();
				if (block.Error != null)
                {
					return new FlowTransactionResponse
					{
						Error = new FlowError($"Transaction failed - could not get latest block. {block.Error.Message}", block.Error.Exception)
					};
				}
				
				SdkAccount sdkAccount = FlowSDK.GetWalletProvider().GetAuthenticatedAccount();
				FlowAccount authAccount = await Accounts.GetByAddress(sdkAccount.Address);
				
				if (authAccount.Error != null)
                {
					return new FlowTransactionResponse
					{
						Error = new FlowError($"Transaction failed - could not get proposer account. {authAccount.Error.Message}", authAccount.Error.Exception)
					};
                }

				if (authAccount.Keys.Count < 1)
				{
					return new FlowTransactionResponse
					{
						Error = new FlowError("Transaction failed - No proposer keys found.")
					};
				}

				//If this key had encountered a sequence error, we delay further transactions using it to give the chain time to catch up and reset the cached value.
				if (SequenceRecoverySet.Contains(authAccount.Keys[0].PublicKey))
				{
					await Task.Delay(SequenceNumberRecoveryDelay);
					authAccount = await Accounts.GetByAddress(authAccount.Address);
					if (authAccount.Error != null)
					{
						return new FlowTransactionResponse
						{
							Error = new FlowError($"Transaction failed - could not get proposer account. {authAccount.Error.Message}", authAccount.Error.Exception)
						};
					}

					SequenceRecoverySet.Remove(authAccount.Keys[0].PublicKey);
					SequenceNumberCache.Remove(authAccount.Keys[0].PublicKey);
				}


				//Use or populate the sequence number cache for this public key.  This allows us to submit a new transaction before the last one is sealed without encountering sequence number errors.
				if (SequenceNumberCache.ContainsKey(authAccount.Keys[0].PublicKey) && authAccount.Keys[0].SequenceNumber <= SequenceNumberCache[authAccount.Keys[0].PublicKey])
				{
					authAccount.Keys[0].SequenceNumber = SequenceNumberCache[authAccount.Keys[0].PublicKey] + 1;
				}

				FlowTransactionProposalKey proposalKey = new FlowTransactionProposalKey
				{
					Address = authAccount.Address,
					KeyId = authAccount.Keys[0].Id,
					SequenceNumber = authAccount.Keys[0].SequenceNumber
				};

				FlowTransaction txRequest = new FlowTransaction
				{
					Script = script,
					GasLimit = TransactionGasLimit,
					ReferenceBlockId = block.Id,
					Payer = authAccount.Address,
					ProposalKey = proposalKey
				};

				// add arguments to the transaction
				foreach (CadenceBase arg in arguments)
				{
					txRequest.AddArgument(arg);
				}

				// Add authorizers to the transaction. 
				// The number of authorizers must match the number of AuthAccount parameters in the prepare()
				// function of the transaction. 
				txRequest.AddAuthorizer(authAccount.Address);

				signature = await FlowSDK.GetWalletProvider().SignTransactionEnvelope(txRequest);
				if (signature != null)
                {
					txRequest.AddEnvelopeSignature(authAccount.Address, authAccount.Keys[0].Id, signature);
				}
				else
				{
					return new FlowTransactionResponse
					{
						Error = new FlowError("Payer did not approve transaction.")
					};
				}

				FlowTransactionResponse txnResponse = await NetworkClient.GetClient().SubmitTransaction(txRequest);
				if (txnResponse.Error != null)
				{
					return new FlowTransactionResponse
					{
						Error = new FlowError($"Transaction failed - {txnResponse.Error.Message}", txnResponse.Error.Exception)
					};
				}

				//The transaction has been accepted for processing.  At this point, even if it fails, the on-chain
				//sequence number will increment, unless a sequence number mismatch occurs
				SequenceNumberCache[authAccount.Keys[0].PublicKey] = authAccount.Keys[0].SequenceNumber;
				
				//Remove any completed monitoring tasks
				SequenceErrorMonitoringTasks.RemoveAll(task => task.IsCompleted);
				
				//Add a new Task to poll the transaction results to see if a sequence number mismatch occurred
				SequenceErrorMonitoringTasks.Add(Task.Run(async () =>
				{
				  DateTime startTime = DateTime.Now;
				  while (true)
				  {
					  await Task.Delay(2000);
					  try
					  {
						  FlowTransactionResult txResult = await Transactions.GetResult(txnResponse.Id);

						  //Error code 1007 indicates a sequence number mismatch occurred
						  if (txResult.ErrorMessage.Contains("[Error Code: 1007]"))
						  {
							  SequenceRecoverySet.Add(authAccount.Keys[0].PublicKey);
							  return;
						  }

						  //If the transaction sealed, there was no mismatch.
						  if (txResult.Status >= FlowTransactionStatus.SEALED)
						  {
							  return;
						  };
					  }
					  catch
				      {
						  if((DateTime.Now - startTime).TotalMilliseconds > 30000)
				          {
							  return;
				          }
					  }
				  }
				}));

				return txnResponse;
			}
			catch (Exception ex)
			{
				return new FlowTransactionResponse()
				{
					Error = new FlowError($"Transaction Submit encountered an error. {ex.Message}", ex)
				};
			}
		}

		/// <summary>
		/// Submits a transaction to the blockchain and waits until target status is achieved before returning
		/// </summary>
		/// <param name="script">The text contents of the transaction to execute</param>
		/// <param name="arguments">Cadence arguments that will be passed to the transaction</param>
		/// <param name="onSubmitSuccess">Callback that will be called once the transaction is successfully submitted, passes in the transaction ID as a parameter</param>
		/// <returns>A Task that will resolve to a FlowTransactionResult upon completion</returns>
		private static async Task<FlowTransactionResult> SubmitAndWaitUntil(FlowTransactionStatus status, string script, List<CadenceBase> arguments, Action<string> onSubmitSuccess = null)
		{
			FlowTransactionResponse response = await Submit(script, arguments);

			if (response.Error != null)
			{
				return new FlowTransactionResult
				{
					Error = response.Error
				};
			}

            if (onSubmitSuccess != null)
            {
				onSubmitSuccess(response.Id);
            }

			FlowTransactionResult result = null;
			FlowTransactionStatus txnStatus = FlowTransactionStatus.UNKNOWN;
			while (txnStatus < status)
			{
				await Task.Delay(2000);
				result = await GetResult(response.Id);
				txnStatus = result.Status;

				if (result.Error != null || result.ErrorMessage != string.Empty)
				{
					break;
				}
			}

			return result;
		}

		/// <summary>
		/// Submits a transaction to the blockchain and waits until executed before returning
		/// </summary>
		/// <param name="script">The text contents of the transaction to execute</param>
		/// <param name="arguments">Cadence arguments that will be passed to the transaction</param>
		/// <returns>A Task that will resolve to a FlowTransactionResult upon completion</returns>
		public static async Task<FlowTransactionResult> SubmitAndWaitUntilExecuted(string script, params CadenceBase[] arguments)
		{
			return await SubmitAndWaitUntil(FlowTransactionStatus.EXECUTED, script, new List<CadenceBase>(arguments));
		}

		/// <summary>
		/// Submits a transaction to the blockchain and waits until sealed before returning
		/// </summary>
		/// <param name="script">The text contents of the transaction to execute</param>
		/// <param name="arguments">Cadence arguments that will be passed to the transaction</param>
		/// <returns>A Task that will resolve to a FlowTransactionResult upon completion</returns>
		public static async Task<FlowTransactionResult> SubmitAndWaitUntilSealed(string script, params CadenceBase[] arguments)
		{
			return await SubmitAndWaitUntil(FlowTransactionStatus.SEALED, script, new List<CadenceBase>(arguments));
		}

		/// <summary>
		/// Submits a transaction to the blockchain and waits until executed before returning
		/// </summary>
		/// <param name="onSubmitSuccess">Callback that will be called once the transaction is successfully submitted, passes in the transaction ID as a parameter</param>
		/// <param name="script">The text contents of the transaction to execute</param>
		/// <param name="arguments">Cadence arguments that will be passed to the transaction</param>
		/// <returns>A Task that will resolve to a FlowTransactionResult upon completion</returns>
		public static async Task<FlowTransactionResult> SubmitAndWaitUntilExecuted(Action<string> onSubmitSuccess, string script, params CadenceBase[] arguments)
		{
			return await SubmitAndWaitUntil(FlowTransactionStatus.EXECUTED, script, new List<CadenceBase>(arguments), onSubmitSuccess);
		}

		/// <summary>
		/// Submits a transaction to the blockchain and waits until sealed before returning
		/// </summary>
		/// <param name="onSubmitSuccess">Callback that will be called once the transaction is successfully submitted, passes in the transaction ID as a parameter</param>
		/// <param name="script">The text contents of the transaction to execute</param>
		/// <param name="arguments">Cadence arguments that will be passed to the transaction</param>
		/// <returns>A Task that will resolve to a FlowTransactionResult upon completion</returns>
		public static async Task<FlowTransactionResult> SubmitAndWaitUntilSealed(Action<string> onSubmitSuccess, string script, params CadenceBase[] arguments)
        {
			return await SubmitAndWaitUntil(FlowTransactionStatus.SEALED, script, new List<CadenceBase>(arguments), onSubmitSuccess);
		}

		/// <summary>
		/// Gets a transaction by its ID
		/// </summary>
		/// <param name="transactionId">The ID of the transaction</param>
		/// <returns>A Task that will resolve into a FlowTransaction when complete</returns>
		public static async Task<FlowTransaction> GetById(string transactionId)
        {
			try
			{
				return await NetworkClient.GetClient().GetTransactionById(transactionId);
			}
			catch (Exception ex)
			{
				return new FlowTransaction
				{
					Error = new FlowError($"Transaction GetById failed, transactionId: {transactionId}. {ex.Message}", ex)
				};
			}
        }

		/// <summary>
		/// Gets the result of a transaction
		/// </summary>
		/// <param name="transactionId">The ID of the transaction</param>
		/// <returns>A Task that will resolve into a FlowTransactionResult when complete</returns>
        public static async Task<FlowTransactionResult> GetResult(string transactionId)
        {
			try
			{
				return await NetworkClient.GetClient().GetTransactionResult(transactionId);
			}
			catch (Exception ex)
			{
				return new FlowTransactionResult()
				{
					Error = new FlowError($"Transaction GetResult failed, transactionId: {transactionId}. {ex.Message}", ex)
				};
			}
		}
    }
}
