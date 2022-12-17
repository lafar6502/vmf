namespace VMF.BusinessObjects
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Data;
    using Sooda;
    using VMFBusinessObjectsStubs = VMF.BusinessObjects.Stubs;
    
    
    public class SysUser : VMFBusinessObjectsStubs.SysUser_Stub
    {
        
        public SysUser(SoodaConstructor c) : 
                base(c)
        {
            // Do not modify this constructor.
        }
        
        public SysUser(SoodaTransaction transaction) : 
                base(transaction)
        {
            // 
            // TODO: Add construction logic here.
            // 
        }
        
        public SysUser() : 
                this(SoodaTransaction.ActiveTransaction)
        {
            // Do not modify this constructor.
        }
    }
}
