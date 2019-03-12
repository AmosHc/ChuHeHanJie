using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

[ProtoContract]
public class LOG_IN
{
    [ProtoMember(1)]
    public string Id { get; set; }

    [ProtoMember(2)]
    public string Password { get; set; }

    public LOG_IN() {
    }
}

[ProtoContract]
public class SIGN_IN
{
    [ProtoMember(1)]
    public string Id { get; set; }

    [ProtoMember(2)]
    public string Password { get; set; }

    [ProtoMember(3)]
    public string name { get; set; }

    public SIGN_IN(){
    }
}

enum _RequestType
{
    SIGN_IN = 101,
    LOG_IN = 102,
    OK = 200,
    FAIL = 201
}
