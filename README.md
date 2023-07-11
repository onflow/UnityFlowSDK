# Flow SDK for Unity

Welcome to the Flow SDK for Unity. This SDK allows Unity developers to integrate their games and apps with the Flow blockchain. Some of the things you can do include: 

- Mint, burn and trade NFTs and Fungible Tokens
- Store game data on-chain
- Read any public data on Flow
- Execute game logic on-chain

You can run your game completely on-chain, or go with a mixed on-chain\off-chain architecture. 

## Overview

The SDK is comprised of two parts:

The **DapperLabs.Flow.Sdk** namespace

This namespace contains pure C# functions that can be used by any .NET program.  It contains the low level primitives needed to connect to and interact with the Flow blockchain.

The **DapperLabs.Flow.Sdk.Unity** namespace

This namespace provides Unity specific helper functions and utilities to make developing Unity apps that run on Flow easier.  It offers an account-centric model of interacting with the Flow network.  

### Which should I use?
You can use either, or both, depending on your needs.
The base Flow SDK offers more control and more features such as allowing for multi-party signing of transactions.
FlowControl offers a simpler workflow for the most commonly used features (executing scripts and single party transactions).

A typical Flow SDK transaction submittal would look like:

- Construct an SdkAccount object
- Connect to the desired network
- Execute a transaction using the Submit function, passing in the script, arguments and accounts to use.
- Wait until transaction is submitted.
- Check for errors.
- Get the Transaction ID of the submitted transaction.
- Poll the blockchain to determine when the transaction finishes executing to get its status and associated events.
- Check for errors.
- Process any events as required.

Using FlowControl it would look like:

- Construct a FlowControl account (either in code or via GUI)
- Use the account to submit the transaction (`account.SubmitAndWaitUntilSealed`)
- Wait until the transaction returns.
- Check for errors.
- Process any events as required.

Under the hood, FlowControl handles connecting to the network associated with the account, submitting the transaction, and polling to determine when the transaction has finished executing.

You can use FlowControl for the majority of things and the Flow SDK for advanced functionality not provided by FlowControl.   

### Asynchronous programming
Because response times when interacting with the blockchain can take seconds, functions that interact with the chain will usually return asyncronous Tasks.  A common pattern for asyncronous programming in Unity is to use Coroutines to wait until the Task completes and act upon it.

Example:

```
void Start()
{
    StartCoroutine(DoQuery());
}

IEnumerator DoQuery()
{
    Task<FlowScriptResponse> task = account.ExecuteScript(queryScript.text);
    yield return new WaitUntil(()=>task.IsCompleted)
    
    if(task.Result.Error == null)
    {
        Debug.Log(task.Result.Value.As<CadenceString>().Value);
    }
    else
    {
        Debug.Log($"Error executing script: {task.Result.Error.Message}");
    }
}
```

### FlowControl

The FlowControl component and editor window is the heart of the Unity editor integration.  

See the [Flow Control documentation](https://developers.flow.com/tools/unity-sdk/guides/flow-control) for more information.

## Requirements

The Flow SDK for Unity requires **Unity 2021.3** or later. The following platforms are currently supported: 

- Windows
- MacOS
- Android
- iOS

## Documentation

Full documentation for the Flow SDK for Unity can be found [here](https://developers.flow.com/tools/unity-sdk). 

[Flow Control](https://developers.flow.com/tools/unity-sdk/guides/flow-control)

[Sample - Flow SDK Usage](https://developers.flow.com/tools/unity-sdk/samples/ui-usage)

[Tutorial - How to build Flow Words game](https://developers.flow.com/tools/unity-sdk/samples/flow-words-tutorial)

[API Reference](https://unity-flow-sdk-api-docs.vercel.app/)
