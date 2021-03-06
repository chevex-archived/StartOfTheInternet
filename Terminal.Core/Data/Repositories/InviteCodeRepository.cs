﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Terminal.Core;
using Terminal.Core.Data.Entities;

namespace Terminal.Core.Data.Repositories
{
    public class InviteCodeRepository : IInviteCodeRepository
    {
        /// <summary>
        /// Every repository requires an instance of the Entity Framework data context.
        /// </summary>
        EntityContainer _entityContainer;

        public InviteCodeRepository(EntityContainer entityContainer)
        {
            _entityContainer = entityContainer;
        }

        public void AddInviteCode(InviteCode inviteCode)
        {
            _entityContainer.InviteCodes.Add(inviteCode);
        }

        public void DeleteInviteCode(InviteCode inviteCode)
        {
            _entityContainer.InviteCodes.Remove(inviteCode);
        }

        public InviteCode GetInviteCode(string code)
        {
            return _entityContainer.InviteCodes.SingleOrDefault(x => x.Code.ToUpper() == code.ToUpper());
        }
    }

    public interface IInviteCodeRepository
    {
        void AddInviteCode(InviteCode inviteCode);
        void DeleteInviteCode(InviteCode inviteCode);
        InviteCode GetInviteCode(string code);
    }
}