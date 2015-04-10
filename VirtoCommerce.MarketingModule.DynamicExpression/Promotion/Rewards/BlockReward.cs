﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VirtoCommerce.Domain.Marketing.Model;
using VirtoCommerce.Foundation.Frameworks.Extensions;

namespace VirtoCommerce.MarketingModule.DynamicExpression.Promotion
{
	public class RewardBlock : DynamicExpressionBlock, IRewardExpression
	{
		#region IRewardsExpression Members

		public PromotionReward[] GetRewards()
		{
			var retVal = new PromotionReward[] { };
			if (Children != null)
			{
				 retVal = Children.OfType<IRewardExpression>().SelectMany(x => x.GetRewards()).ToArray();
			}
			return retVal;
		}

		#endregion
	}
}