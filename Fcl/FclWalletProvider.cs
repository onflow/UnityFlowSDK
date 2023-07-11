using Fcl.Net.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DapperLabs.Flow.Sdk.Fcl
{
    public class FclWalletProvider
    {
        public string Name { get; set; }
        public string Logo { get; set; }
        public FclServiceMethod Method { get; set; }
        public string Endpoint { get; set; }
    }
}
