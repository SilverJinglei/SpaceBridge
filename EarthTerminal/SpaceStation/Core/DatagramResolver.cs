using System;
using System.Collections.Generic;
using System.Linq;

namespace SpaceStation.Core
{
    internal abstract class DatagramResolverBase
    {
        /// <summary>
        /// legacy for not use ConnectedStream.DataAvailable
        /// </summary>
        public abstract IEnumerable<string> Resolve(ref string lastPiece);
    }

    internal class JsonResolver : DatagramResolverBase
    {
        const string JsonSplitter = "}{";
        const char LeftBrace = '{';
        const char RightBrace = '}';

        public static bool IsJsonValid(string json)
        {
            // 1. json exception strategy
            //try
            //{
            //    JToken.Parse(json);
            //    return true;
            //}
            //catch (Exception)
            //{
            //    return false;
            //}

            // 2. smart pointer count strategy
            int count = 0;
            foreach (var c in json)
            {
                switch (c)
                {
                    case LeftBrace:
                        count ++;
                        break;
                    case RightBrace:
                        count --;
                        break;
                }
            }

            return count == 0;

            // 3. linq count strategy
            var leftBraceCount = json.Count(j => j == LeftBrace);
            var rightBraceCount = json.Count(j => j == RightBrace);

            return leftBraceCount == rightBraceCount;
            //return json.EndsWith(RightBrace);
        }

        void Complete_FirstPiece_withRightBrace()
            => _datagrams[0] += RightBrace;

        void Complete_MiddlePieces_withLeftRightBraces()
        {
            const int secondIndex = 1;

            for (var i = secondIndex; i < _datagrams.Count - 1; i++)
                _datagrams[i] = LeftBrace + _datagrams[i] + RightBrace;
        }

        void Complete_LastPiece_withLeftBrace()
        {
            var lastIndex = _datagrams.Count - 1;
            _datagrams[lastIndex] = LeftBrace + _datagrams[lastIndex];
        }

        private void Complete_RemainingPiece(out string remainingPiece)
        {
            var lastPiece = _datagrams[_datagrams.Count - 1];

            if (IsJsonValid(lastPiece))
            {
                remainingPiece = string.Empty;
                return;
            }

            remainingPiece = lastPiece;
            _datagrams.Remove(lastPiece);
        }

        private List<string> _datagrams;

        /// <summary>
        /// legacy for not use ConnectedStream.DataAvailable
        /// </summary>
        public override IEnumerable<string> Resolve(ref string remainingPiece)
        {
            _datagrams = remainingPiece.Split(new[] {JsonSplitter}, StringSplitOptions.None).ToList();

            if (_datagrams.Count > 1)
            {
                Complete_FirstPiece_withRightBrace();
                Complete_MiddlePieces_withLeftRightBraces();
                Complete_LastPiece_withLeftBrace();
            }

            Complete_RemainingPiece(out remainingPiece);
            return _datagrams;
        }
    }

    internal class SimpleHeaderResolver : DatagramResolverBase
    {
        public override IEnumerable<string> Resolve(ref string lastPiece)
        {
            var bytesCount = lastPiece[0];

            return new string[0];
        }
    }
}