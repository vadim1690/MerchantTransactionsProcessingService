using MerchantTransactionProcessing.Data.Entities;

namespace MerchantTransactionProcessing.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // migrate

            if (!dbContext.Merchants.Any())
            {
                var now = DateTime.UtcNow;
                var merchants = new[]
                {
                    new Merchant { Name = "Alpha Store", CreatedAt = now, ModifiedAt = now },
                    new Merchant { Name = "Beta Boutique", CreatedAt = now, ModifiedAt = now },
                    new Merchant { Name = "Gamma Goods", CreatedAt = now, ModifiedAt = now },
                    new Merchant { Name = "Delta Digital", CreatedAt = now, ModifiedAt = now },
                    new Merchant { Name = "Epsilon Electronics", CreatedAt = now, ModifiedAt = now },
                    new Merchant { Name = "Zeta Zone", CreatedAt = now, ModifiedAt = now },
                    new Merchant { Name = "Eta Eatery", CreatedAt = now, ModifiedAt = now },
                    new Merchant { Name = "Theta Theater", CreatedAt = now, ModifiedAt = now },
                    new Merchant { Name = "Iota Imports", CreatedAt = now, ModifiedAt = now },
                    new Merchant { Name = "Kappa Kitchen", CreatedAt = now, ModifiedAt = now },
                    new Merchant { Name = "Lambda Luxury", CreatedAt = now, ModifiedAt = now },
                    new Merchant { Name = "Mu Markets", CreatedAt = now, ModifiedAt = now }
                };
                dbContext.Merchants.AddRange(merchants);
                await dbContext.SaveChangesAsync();
            }

            if (!dbContext.PaymentMethods.Any())
            {
                var now = DateTime.UtcNow;
                // Reload merchants so we have their IDs
                var merchants = dbContext.Merchants.ToList();

                var methods = new List<PaymentMethod>();

                // Common payment methods
                var commonMethods = new[] { "Credit Card", "Debit Card", "PayPal", "Cash", "Bank Transfer", "Cryptocurrency", "Apple Pay", "Google Pay", "Gift Card" };

                // Add methods for each merchant with some variations
                foreach (var merchant in merchants)
                {
                    // Each merchant gets 3-6 payment methods
                    var methodCount = new Random().Next(3, 7);
                    var shuffledMethods = commonMethods.OrderBy(x => Guid.NewGuid()).Take(methodCount).ToList();

                    foreach (var method in shuffledMethods)
                    {
                        methods.Add(new PaymentMethod
                        {
                            MerchantId = merchant.Id,
                            Method = method,
                            MethodDetails = Guid.NewGuid().ToString(),
                            CreatedAt = now.AddDays(-new Random().Next(1, 30)),
                            ModifiedAt = now
                        });
                    }
                }

                dbContext.PaymentMethods.AddRange(methods);
                await dbContext.SaveChangesAsync();
            }

            if (!dbContext.Transactions.Any())
            {
                var now = DateTime.UtcNow;
                var merchants = dbContext.Merchants.ToList();
                var paymentMethods = dbContext.PaymentMethods.ToList();
                var random = new Random();

                var statuses = new[] { "Completed", "Pending", "Failed", "Refunded", "Disputed", "Processing" };
                var transactions = new List<Transaction>();

                // Create 5-15 transactions for each merchant
                foreach (var merchant in merchants)
                {
                    var merchantPaymentMethods = paymentMethods.Where(pm => pm.MerchantId == merchant.Id).ToList();
                    var transactionCount = random.Next(5, 16);

                    for (int i = 0; i < transactionCount; i++)
                    {
                        // Random payment method for this merchant
                        var paymentMethod = merchantPaymentMethods[random.Next(merchantPaymentMethods.Count)];

                        // Random date within the last 60 days
                        var daysAgo = random.Next(0, 61);
                        var hoursAgo = random.Next(0, 24);
                        var minutesAgo = random.Next(0, 60);
                        var transactionDate = now.AddDays(-daysAgo).AddHours(-hoursAgo).AddMinutes(-minutesAgo);

                        // Random amount between $5 and $1000
                        var amount = Math.Round((decimal)(random.NextDouble() * 995 + 5), 2);

                        // Random status
                        var status = statuses[random.Next(statuses.Length)];

                        transactions.Add(new Transaction
                        {
                            MerchantId = merchant.Id,
                            PaymentMethodId = paymentMethod.Id,
                            TransactionDate = transactionDate,
                            Amount = amount,
                            Status = status,
                            CreatedAt = transactionDate,
                            ModifiedAt = status == "Pending" || status == "Processing" ? transactionDate : transactionDate.AddMinutes(random.Next(5, 120))
                        });
                    }
                }

                // Add a few high-value transactions
                for (int i = 0; i < 5; i++)
                {
                    var merchant = merchants[random.Next(merchants.Count)];
                    var merchantPaymentMethods = paymentMethods.Where(pm => pm.MerchantId == merchant.Id).ToList();
                    var paymentMethod = merchantPaymentMethods[random.Next(merchantPaymentMethods.Count)];

                    var daysAgo = random.Next(0, 31);
                    var transactionDate = now.AddDays(-daysAgo);

                    // Amount between $1000 and $10000
                    var amount = Math.Round((decimal)(random.NextDouble() * 9000 + 1000), 2);

                    transactions.Add(new Transaction
                    {
                        MerchantId = merchant.Id,
                        PaymentMethodId = paymentMethod.Id,
                        TransactionDate = transactionDate,
                        Amount = amount,
                        Status = "Completed",
                        CreatedAt = transactionDate,
                        ModifiedAt = transactionDate.AddMinutes(random.Next(5, 60))
                    });
                }

                // Add some very recent transactions (last 24 hours)
                for (int i = 0; i < 10; i++)
                {
                    var merchant = merchants[random.Next(merchants.Count)];
                    var merchantPaymentMethods = paymentMethods.Where(pm => pm.MerchantId == merchant.Id).ToList();
                    var paymentMethod = merchantPaymentMethods[random.Next(merchantPaymentMethods.Count)];

                    var hoursAgo = random.Next(0, 24);
                    var minutesAgo = random.Next(0, 60);
                    var transactionDate = now.AddHours(-hoursAgo).AddMinutes(-minutesAgo);

                    // Random amount between $5 and $500
                    var amount = Math.Round((decimal)(random.NextDouble() * 495 + 5), 2);

                    // Most recent transactions are more likely to be pending
                    var statusOptions = new[] { "Completed", "Pending", "Processing", "Pending", "Processing" };
                    var status = statusOptions[random.Next(statusOptions.Length)];

                    transactions.Add(new Transaction
                    {
                        MerchantId = merchant.Id,
                        PaymentMethodId = paymentMethod.Id,
                        TransactionDate = transactionDate,
                        Amount = amount,
                        Status = status,
                        CreatedAt = transactionDate,
                        ModifiedAt = transactionDate
                    });
                }

                dbContext.Transactions.AddRange(transactions);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}