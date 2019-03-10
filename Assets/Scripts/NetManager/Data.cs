using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

[ProtoContract]
public class Login
{
    [ProtoMember(1)]
    public string id { get; set; }

    [ProtoMember(2)]
    public string password { get; set; }

    public Login() { }
}