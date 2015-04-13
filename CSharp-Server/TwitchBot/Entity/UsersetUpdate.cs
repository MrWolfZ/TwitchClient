using System;
using System.Collections.Immutable;
using System.Linq;

namespace TwitchBot.Entity
{
    public struct UsersetUpdate
    {
        private readonly IImmutableSet<User> joins;
        private readonly IImmutableSet<User> newUserSet;
        private readonly IImmutableSet<User> oldUserSet;
        private readonly IImmutableSet<User> parts;

        public UsersetUpdate(IImmutableSet<User> oldUserSet, IImmutableSet<User> newUserSet, IImmutableSet<User> joins, IImmutableSet<User> parts)
            : this()
        {
            this.oldUserSet = oldUserSet;
            this.newUserSet = newUserSet;
            this.joins = joins;
            this.parts = parts;
        }

        public IImmutableSet<User> Joins
        {
            get { return this.joins; }
        }

        public IImmutableSet<User> NewUserSet
        {
            get { return this.newUserSet; }
        }

        public IImmutableSet<User> OldUserSet
        {
            get { return this.oldUserSet; }
        }

        public IImmutableSet<User> Parts
        {
            get { return this.parts; }
        }
    }
}