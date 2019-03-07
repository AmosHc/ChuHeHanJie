using Google.Protobuf;
using Google.Protobuf.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Data
{
    public enum RequestType
    {
        LogIn,
        Register
    }

    public class GetRequestType:IMessage
    {
        public RequestType RequestType;

        public MessageDescriptor Descriptor => throw new NotImplementedException();

        public int CalculateSize()
        {
            throw new NotImplementedException();
        }

        public void MergeFrom(CodedInputStream input)
        {
            RequestType = (RequestType)input.ReadEnum();
        }

        public void WriteTo(CodedOutputStream output)
        {
            throw new NotImplementedException();
        }
    }

    public class LogIn:IMessage
    {
        public MessageDescriptor Descriptor => throw new NotImplementedException();

        public int CalculateSize()
        {

            return 0;
        }

        public void MergeFrom(CodedInputStream input)
        {
            id = input.ReadString();
            password = input.ReadString();
        }

        public void WriteTo(CodedOutputStream output)
        {
            output.WriteString(id);
            output.WriteString(password);
        }

        public RequestType RequestType;

        public string id;

        public string password;
    }
}
