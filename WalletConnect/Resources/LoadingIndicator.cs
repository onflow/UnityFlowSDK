using UnityEngine;

namespace DapperLabs.Flow.Sdk.WalletConnect
{
    /// <summary>
    /// Loading Indicator graphic used on dialogs. 
    /// </summary>
    public class LoadingIndicator : MonoBehaviour
    {
        public float RotationSpeed = 360.0f;
        public float RotationInterval = 45.0f;

        private float _RotationStepProgress = 0.0f;

        void Update()
        {
            _RotationStepProgress += Time.deltaTime * RotationSpeed;

            while (_RotationStepProgress >= RotationInterval)
            {
                transform.Rotate(Vector3.forward, -RotationInterval);
                _RotationStepProgress -= RotationInterval;
            }
        }
    }
}
