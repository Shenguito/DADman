using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComLibrary
{
    [Serializable]
    public class InvalidePortException : ApplicationException
    {

        public InvalidePortException() { }

        public InvalidePortException(string msg)
            : base(msg)
        {
        }

        public InvalidePortException(System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
        {
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }

    [Serializable]
    public class InvalideUsernameException : ApplicationException
    {

        public InvalideUsernameException() { }

        public InvalideUsernameException(string msg)
            : base(msg)
        {
        }

        public InvalideUsernameException(System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
        {
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }

    [Serializable]
    public class ThereIsNoCommunication : ApplicationException
    {

        public ThereIsNoCommunication() { }

        public ThereIsNoCommunication(string msg)
            : base(msg)
        {
        }
        public ThereIsNoCommunication(System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
        {
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }

    [Serializable]
    public class ClientIsDown : ApplicationException
    {

        public ClientIsDown() { }

        public ClientIsDown(string msg)
            : base(msg)
        {
        }
        public ClientIsDown(System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
        {
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
