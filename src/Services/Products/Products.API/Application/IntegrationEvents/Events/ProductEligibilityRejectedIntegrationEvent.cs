﻿
using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Events;

namespace eRewards.Services.Products.API.Application.IntegrationEvents.Events
{
    /// <summary>
    /// 
    /// </summary>
    public record ProductEligibilityRejectedIntegrationEvent : IntegrationEvent
    {
        /// <summary>
        /// 
        /// </summary>
        public int AccountNo { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int ActionRecordId { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountNo"></param>
        /// <param name="actionRecordId"></param>
        public ProductEligibilityRejectedIntegrationEvent(int accountNo, int actionRecordId)
        {
            AccountNo = accountNo;
            ActionRecordId = actionRecordId;
        }
    }
}
