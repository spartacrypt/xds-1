using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UnnamedCoin.Bitcoin.Consensus;
using UnnamedCoin.Bitcoin.Consensus.Rules;
using UnnamedCoin.Bitcoin.Features.Consensus.CoinViews;

namespace UnnamedCoin.Bitcoin.Features.Consensus.Rules.CommonRules
{
    public class SaveCoinviewRule : UtxoStoreConsensusRule
    {
        /// <summary>
        ///     Specifies time threshold which is used to determine if flush is required.
        ///     When consensus tip timestamp is greater than current time minus the threshold the flush is required.
        /// </summary>
        /// <remarks>Used only on blockchains without max reorg property.</remarks>
        const int FlushRequiredThresholdSeconds = 2 * 24 * 60 * 60;

        /// <inheritdoc />
        public override async Task RunAsync(RuleContext context)
        {
            var oldBlockHash = context.ValidationContext.ChainedHeaderToValidate.Previous.HashBlock;
            var nextBlockHash = context.ValidationContext.ChainedHeaderToValidate.HashBlock;
            var height = context.ValidationContext.ChainedHeaderToValidate.Height;

            // Persist the changes to the coinview. This will likely only be stored in memory,
            // unless the coinview treashold is reached.
            this.Logger.LogDebug("Saving coinview changes.");
            var utxoRuleContext = context as UtxoRuleContext;
            this.PowParent.UtxoSet.SaveChanges(utxoRuleContext.UnspentOutputSet.GetCoins(), null, oldBlockHash,
                nextBlockHash, height);

            // Use the default flush condition to decide if flush is required (currently set to every 60 seconds)
            if (this.PowParent.UtxoSet is CachedCoinView cachedCoinView)
                cachedCoinView.Flush(false);
        }
    }

    public class LoadCoinviewRule : UtxoStoreConsensusRule
    {
        /// <inheritdoc />
        public override async Task RunAsync(RuleContext context)
        {
            // Check that the current block has not been reorged.
            // Catching a reorg at this point will not require a rewind.
            if (context.ValidationContext.BlockToValidate.Header.HashPrevBlock !=
                this.Parent.ChainState.ConsensusTip.HashBlock)
            {
                this.Logger.LogDebug("Reorganization detected.");
                ConsensusErrors.InvalidPrevTip.Throw();
            }

            var utxoRuleContext = context as UtxoRuleContext;

            // Load the UTXO set of the current block. UTXO may be loaded from cache or from disk.
            // The UTXO set is stored in the context.
            this.Logger.LogDebug("Loading UTXO set of the new block.");
            utxoRuleContext.UnspentOutputSet = new UnspentOutputSet();

            var ids = this.coinviewHelper.GetIdsToFetch(context.ValidationContext.BlockToValidate,
                context.Flags.EnforceBIP30);
            var coins = this.PowParent.UtxoSet.FetchCoins(ids);
            utxoRuleContext.UnspentOutputSet.SetCoins(coins.UnspentOutputs);
        }
    }
}