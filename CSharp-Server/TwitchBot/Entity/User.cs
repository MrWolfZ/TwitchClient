using System;
using System.Linq;

namespace TwitchBot.Entity
{
    public struct User
    {
        private readonly string name;

        public User(string name) : this()
        {
            this.name = name;
        }

        public string Name
        {
            get { return this.name; }
        }

        public bool Equals(User other)
        {
            return string.Equals(this.name, other.name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is User && this.Equals((User)obj);
        }

        public override int GetHashCode()
        {
            return this.name != null ? this.name.GetHashCode() : 0;
        }
    }
}