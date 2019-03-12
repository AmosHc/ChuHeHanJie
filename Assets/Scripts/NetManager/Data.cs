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
    public string id { get; set; }

    [ProtoMember(2)]
    public string password { get; set; }

    public LOG_IN() {
    }
}

enum _RequestType
{
    SIGN_IN = 101,
    LOG_IN = 102,
    OK = 200
}