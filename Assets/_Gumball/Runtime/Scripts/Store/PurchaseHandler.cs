using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public struct PurchaseHandler
    {
    
        public readonly Action onSuccess;
        public readonly Action onFail;
            
        public PurchaseHandler(Action onSuccess, Action onFail)
        {
            this.onSuccess = onSuccess;
            this.onFail = onFail;
        }
        
    }
}
