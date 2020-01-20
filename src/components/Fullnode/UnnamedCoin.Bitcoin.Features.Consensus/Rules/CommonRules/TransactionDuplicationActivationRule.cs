﻿using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UnnamedCoin.Bitcoin.Consensus;
using UnnamedCoin.Bitcoin.Consensus.Rules;

namespace UnnamedCoin.Bitcoin.Features.Consensus.Rules.CommonRules
{
    /// <summary>
    ///     Prevent duplicate transactions in the coinbase.
    /// </summary>
    /// <remarks>
    ///     More info here https://github.com/bitcoin/bips/blob/master/bip-0030.mediawiki
    /// </remarks>
    public class TransactionDuplicationActivationRule : UtxoStoreConsensusRule
    {
        /// <inheritdoc />
        /// >
        /// <exception cref="ConsensusErrors.BadTransactionBIP30"> Thrown if BIP30 is not passed.</exception>
        public override Task RunAsync(RuleContext context)
        {
            if (!context.SkipValidation)
            {
                var block = context.ValidationContext.BlockToValidate;
                var flags = context.Flags;
                var utxoRuleContext = context as UtxoRuleContext;
                var view = utxoRuleContext.UnspentOutputSet;

                if (flags.EnforceBIP30)
                    foreach (var tx in block.Transactions)
                    {
                        var coins = view.AccessCoins(tx.GetHash());
                        if (coins != null && !coins.IsPrunable)
                        {
                            this.Logger.LogDebug("Transaction '{0}' already found in store", tx.GetHash());
                            this.Logger.LogTrace("(-)[BAD_TX_BIP_30]");
                            ConsensusErrors.BadTransactionBIP30.Throw();
                        }
                    }
            }
            else
            {
                this.Logger.LogDebug("BIP30 validation skipped for checkpointed block at height {0}.",
                    context.ValidationContext.ChainedHeaderToValidate.Height);
            }

            return Task.CompletedTask;
        }
    }
}