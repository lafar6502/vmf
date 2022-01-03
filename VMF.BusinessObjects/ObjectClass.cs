namespace VMF.BusinessObjects
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Data;
    using Sooda;
    using VMFBusinessObjectsStubs = VMF.BusinessObjects.Stubs;
    
    
    public class ObjectClass : VMFBusinessObjectsStubs.ObjectClass_Stub
    {
        
        public ObjectClass(SoodaConstructor c) : 
                base(c)
        {
            // Do not modify this constructor.
        }
        
        public ObjectClass(SoodaTransaction transaction) : 
                base(transaction)
        {
            // 
            // TODO: Add construction logic here.
            // 
        }
        
        public ObjectClass() : 
                this(SoodaTransaction.ActiveTransaction)
        {
            // Do not modify this constructor.
        }
    }
}
