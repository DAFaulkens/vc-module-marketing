﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VirtoCommerce.Domain.Marketing.Model;

namespace VirtoCommerce.MarketingModule.DynamicExpression.Promotion
{
	//Get []% off [] items
	public class RewardItemGetOfRelForNum : DynamicExpression, IRewardExpression
	{
		public decimal Amount { get; set; }
		public string ProductId { get; set; }
		public int NumItem { get; set; }
		#region IRewardExpression Members

		public PromotionReward[] GetRewards()
		{
			var retVal = new CatalogItemAmountReward
			{
				Amount = Amount,
				AmountType = RewardAmountType.Relative,
				Quantity = NumItem,
				ProductId = ProductId
			};
			return new PromotionReward[] { retVal };
		}

		#endregion
	}
}