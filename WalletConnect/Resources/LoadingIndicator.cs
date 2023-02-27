using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DapperLabs.Flow.Sdk.WalletConnect
{
    public class LoadingIndicator : MonoBehaviour
    {
        public float RotationSpeed = 360.0f;
        public float RotationInterval = 45.0f;

        private float _RotationStepProgress = 0.0f;

        // Update is called once per frame
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
