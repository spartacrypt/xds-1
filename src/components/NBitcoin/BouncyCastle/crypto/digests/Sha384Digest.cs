using NBitcoin.BouncyCastle.crypto.util;
using NBitcoin.BouncyCastle.util;

namespace NBitcoin.BouncyCastle.crypto.digests
{
    /**
     * Draft FIPS 180-2 implementation of SHA-384.
     * <b>Note:</b>
     * As this is
     * based on a draft this implementation is subject to change.
     * <pre>
     *     block  word  digest
     *     SHA-1   512    32    160
     *     SHA-256 512    32    256
     *     SHA-384 1024   64    384
     *     SHA-512 1024   64    512
     * </pre>
     */
    class Sha384Digest
        : LongDigest
    {
        const int DigestLength = 48;

        public Sha384Digest()
        {
        }

        /**
         * Copy constructor.  This will copy the state of the provided
         * message digest.
         */
        public Sha384Digest(
            Sha384Digest t)
            : base(t)
        {
        }

        public override string AlgorithmName => "SHA-384";

        public override int GetDigestSize()
        {
            return DigestLength;
        }

        public override int DoFinal(
            byte[] output,
            int outOff)
        {
            Finish();

            Pack.UInt64_To_BE(this.H1, output, outOff);
            Pack.UInt64_To_BE(this.H2, output, outOff + 8);
            Pack.UInt64_To_BE(this.H3, output, outOff + 16);
            Pack.UInt64_To_BE(this.H4, output, outOff + 24);
            Pack.UInt64_To_BE(this.H5, output, outOff + 32);
            Pack.UInt64_To_BE(this.H6, output, outOff + 40);

            Reset();

            return DigestLength;
        }

        /**
        * reset the chaining variables
        */
        public override void Reset()
        {
            base.Reset();

            /* SHA-384 initial hash value
                * The first 64 bits of the fractional parts of the square roots
                * of the 9th through 16th prime numbers
                */
            this.H1 = 0xcbbb9d5dc1059ed8;
            this.H2 = 0x629a292a367cd507;
            this.H3 = 0x9159015a3070dd17;
            this.H4 = 0x152fecd8f70e5939;
            this.H5 = 0x67332667ffc00b31;
            this.H6 = 0x8eb44a8768581511;
            this.H7 = 0xdb0c2e0d64f98fa7;
            this.H8 = 0x47b5481dbefa4fa4;
        }

        public override IMemoable Copy()
        {
            return new Sha384Digest(this);
        }

        public override void Reset(IMemoable other)
        {
            var d = (Sha384Digest) other;

            CopyIn(d);
        }
    }
}