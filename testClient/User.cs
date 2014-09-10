using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace testClient
{
    [Serializable]
    public class User
    {
        public string name{set;get;}
        public int age { set; get; }
        public User(string name,int age)
        {
            this.name = name;
            this.age = age;
        }
    }
}
