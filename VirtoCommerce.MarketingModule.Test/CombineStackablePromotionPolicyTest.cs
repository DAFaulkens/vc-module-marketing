﻿using Moq;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Common;
using VirtoCommerce.Domain.Marketing.Model;
using VirtoCommerce.Domain.Marketing.Model.Promotions.Search;
using VirtoCommerce.Domain.Marketing.Services;
using VirtoCommerce.MarketingModule.Data.Services;
using Xunit;

namespace VirtoCommerce.MarketingModule.Test
{
    [Trait("Category", "CI")]
    public class CombineStackablePromotionPolicyTest 
    {
        [Fact]
        public void EvaluateRewards_CombineByPriorityOrder()
        {     
            //Arrange            
            var evalPolicy = GetPromotionEvaluationPolicy(GetPromotions("FedEx Get 50% Off", "FedEx Get 30% Off", "ProductA and ProductB Get 2 With 50% Off", "Get ProductA With 25$ Off"));
            var productA = new ProductPromoEntry { ProductId = "ProductA", Price = 100, Quantity = 1 };
            var productB = new ProductPromoEntry { ProductId = "ProductB", Price = 100, Quantity = 3 };
            var context = new PromotionEvaluationContext
            {
                 ShipmentMethodCode = "FedEx",
                 ShipmentMethodPrice = 100,
                 PromoEntries = new [] { productA, productB }
            };
            //Act
            var rewards = evalPolicy.EvaluatePromotion(context).Rewards;

            //Assert
            Assert.Equal(rewards.Count(), 5);
            Assert.Equal(context.ShipmentMethodPrice, 35m);
            Assert.Equal(productA.Price, 37.5m);
            Assert.Equal(productB.Price, 50);
        }

        [Fact]
        public void EvaluateRewards_OnlySingleExlusivePromotion()
        {
            //Arrange            
            var evalPolicy = GetPromotionEvaluationPolicy(TestPromotions);
            var productA = new ProductPromoEntry { ProductId = "ProductA", Price = 100, Quantity = 1 };
            var productB = new ProductPromoEntry { ProductId = "ProductB", Price = 100, Quantity = 1 };
            var context = new PromotionEvaluationContext
            {
                ShipmentMethodCode = "FedEx",
                ShipmentMethodPrice = 100,
                PromoEntries = new[] { productA, productB }
            };
            //Act
            var rewards = evalPolicy.EvaluatePromotion(context).Rewards;

            //Assert
            Assert.Single(rewards);
            Assert.Equal(rewards.Single().Promotion.Id, "Exclusive ProductB Get 10$ Off");
        }

        [Fact]
        public void EvaluateRewards_SkipRewardsMakingPriceNegative()
        {
            //Arrange            
            var evalPolicy = GetPromotionEvaluationPolicy(GetPromotions("Get ProductA Free", "Get ProductA With 25$ Off"));
            var productA = new ProductPromoEntry { ProductId = "ProductA", Price = 100, Quantity = 1 };
            var context = new PromotionEvaluationContext
            {
                PromoEntries = new[] { productA }
            };
            //Act
            var rewards = evalPolicy.EvaluatePromotion(context).Rewards;

            //Assert
            Assert.Single(rewards);
            Assert.Equal(rewards.Single().Promotion.Id, "Get ProductA Free");
            Assert.Equal(productA.Price, 0);
        }

        private static IMarketingPromoEvaluator GetPromotionEvaluationPolicy(IEnumerable<Promotion> promotions)
        {
            var result = new GenericSearchResult<Promotion>
            {
                Results = promotions.ToList()
            };
            var promoSearchServiceMock = new Moq.Mock<IPromotionSearchService>();
            promoSearchServiceMock.Setup(x => x.SearchPromotions(It.IsAny<PromotionSearchCriteria>())).Returns(result);

            return new CombineStackablePromotionPolicy(promoSearchServiceMock.Object);
        }

     
        private static IEnumerable<Promotion> TestPromotions
        { 
            get
            {
                yield return new MockPromotion
                {
                    Id = "FedEx Get 50% Off",
                    Rewards = new[]
                    {
                        new ShipmentReward { ShippingMethod = "FedEx", Amount = 50, AmountType = RewardAmountType.Relative, IsValid = true }
                    },
                    Priority = 1,
                    IsExclusive = false
                };
                yield return new MockPromotion
                {
                    Id = "FedEx Get 30% Off",
                    Rewards = new[]
                   {
                        new ShipmentReward { ShippingMethod = "FedEx", Amount = 30, AmountType = RewardAmountType.Relative, IsValid = true  }
                    },
                    Priority = 2,
                    IsExclusive = false
                };
                yield return new MockPromotion
                {
                    Id = "Exclusive ProductB Get 10$ Off",
                    Rewards = new[]
                   {
                        new CatalogItemAmountReward { ProductId = "ProductB", Amount = 10, AmountType = RewardAmountType.Absolute, IsValid = true }
                    },
                    Priority = 10,
                    IsExclusive = true
                };
                yield return new MockPromotion
                {
                    Id = "Get ProductA Free",
                    Rewards = new[]
                    {
                       new CatalogItemAmountReward { ProductId = "ProductA", Amount = 100, AmountType = RewardAmountType.Relative, IsValid = true },                   
                    },
                    Priority = 100,
                    IsExclusive = false
                };
                yield return new MockPromotion
                {
                    Id = "Get ProductA With 25$ Off",
                    Rewards = new[]
                    {
                       new CatalogItemAmountReward { ProductId = "ProductA", Amount = 25, AmountType = RewardAmountType.Absolute, IsValid = true },
                    },
                    Priority = 80,
                    IsExclusive = false
                };
                yield return new MockPromotion
                {
                    Id = "ProductA and ProductB Get 2 With 50% Off",
                    Rewards = new[]
                    {
                       new CatalogItemAmountReward { ProductId = "ProductA", Amount = 50, Quantity = 2, AmountType = RewardAmountType.Relative, IsValid = true },
                       new CatalogItemAmountReward { ProductId = "ProductB", Amount = 50, Quantity = 2, AmountType = RewardAmountType.Relative, IsValid = true}
                    },
                    Priority = 15,
                    IsExclusive = false
                };
                yield return new MockPromotion
                {
                    Id = "Buy Order with 55% Off",
                    Rewards = new[]
                    {
                       new CartSubtotalReward {  Amount = 55, IsValid = true }                   
                    },
                    Priority = 20,
                    IsExclusive = false
                };
                yield return new MockPromotion
                {
                    Id = "Get Gift",
                    Rewards = new[]
                    {
                       new GiftReward {  ProductId = "ProductA", IsValid = true }
                    },
                    Priority = 0,
                    IsExclusive = false
                };
            }
        }

        private static IEnumerable<Promotion> GetPromotions(params string[] ids)
        {
            return TestPromotions.Where(x => ids.Contains(x.Id));
        }

    }

    internal class MockPromotion : Promotion
    {
        public IEnumerable<PromotionReward> Rewards { get; set; }
      
        public override PromotionReward[] EvaluatePromotion(IEvaluationContext context)
        {
            foreach (var reward in Rewards)
            {
                reward.Promotion = this;
            }
            return Rewards.ToArray();
        }
    }
}
