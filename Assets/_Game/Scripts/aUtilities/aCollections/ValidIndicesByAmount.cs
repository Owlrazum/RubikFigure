using System;

namespace Orazum.Collections
{
    public class ValidIndicesByAmount
    {
        private int[] _presentAmount;
        private int _validIndicesCount;
        private int _defaultAmount;

        Func<int, bool> ValidConditionFunc;
        public ValidIndicesByAmount(int capasity, int defaultAmountArg, 
            Func<int, bool> ValidConditionFuncArg)
        {
            _presentAmount = new int[capasity];
            _defaultAmount = defaultAmountArg;
            ValidConditionFunc = ValidConditionFuncArg;
        }

        public int DefaultIndicesCount { get { return _presentAmount.Length; } }
        public int ValidIndicesCount { get { return _validIndicesCount; } }

        public int GetValidIndicesCountExcept(int exceptIndex)
        {
            int count = 0;
            for (int i = 0; i < _presentAmount.Length; i++)
            {
                if (i == exceptIndex)
                {
                    continue;
                }

                if (ValidConditionFunc(_presentAmount[i]))
                {
                    count++;
                }
            }

            return count;
        }

        public int GetValidIndex(int indexOfValidIndex, out int amount)
        {
            int indexer = 0;
            for (int i = 0; i < _presentAmount.Length; i++)
            {
                if (indexer == indexOfValidIndex)
                {
                    amount = _presentAmount[i];
                    return i;
                }
                if (ValidConditionFunc(_presentAmount[i]))
                {
                    indexer++;
                }
            }

            throw new System.ArgumentException("Out of bounds valid index");
        }

        public bool IncrementAmount(int indexOfValidIndex, bool isPotentialInvalidation = false)
        {
            _validIndicesCount = -1;

            int indexer = 0;
            for (int i = 0; i < _presentAmount.Length; i++)
            {
                if (indexer == indexOfValidIndex)
                {
                    _presentAmount[i]++;
                    if (isPotentialInvalidation && !ValidConditionFunc(_presentAmount[i]))
                    {
                        _validIndicesCount--;
                        return true;
                    }
                    return false;
                }
                if (ValidConditionFunc(_presentAmount[i]))
                {
                    indexer++;
                }
            }

            throw new System.ArgumentException("Out of bounds valid index");
        }

        public bool DecrementAmount(int indexOfValidIndex, bool isPotentialInvalidation = true)
        {
            _validIndicesCount = -1;

            int indexer = 0;
            for (int i = 0; i < _presentAmount.Length; i++)
            {
                if (indexer == indexOfValidIndex)
                {
                    _presentAmount[i]--;
                    if (isPotentialInvalidation && !ValidConditionFunc(_presentAmount[i]))
                    {
                        _validIndicesCount--;
                        return true;
                    }
                    return false;
                }
                if (ValidConditionFunc(_presentAmount[i]))
                {
                    indexer++;
                }
            }

            throw new System.ArgumentException("Out of bounds valid index");
        }

        public void Reset()
        {
            for (int i = 0; i < _presentAmount.Length; i++)
            {
                _presentAmount[i] = _defaultAmount;
            }
            _validIndicesCount = _presentAmount.Length;
        }
    }
}