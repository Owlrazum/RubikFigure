namespace Orazum.Collections
{
    public class ValidIndicesByBool
    {
        private bool[] _isInvalidAtIndex;
        private int _validIndicesCount;
        public ValidIndicesByBool(int capasity)
        {
            _isInvalidAtIndex = new bool[capasity];
            _validIndicesCount = capasity;
        }

        public int GetValidIndicesCount()
        {
            return _validIndicesCount;
        }

        public int GetValidIndex(int indexOfValidIndex)
        {
            int indexer = 0;
            for (int i = 0; i < _isInvalidAtIndex.Length; i++)
            {
                if (indexer == indexOfValidIndex)
                {
                    return i;
                }
                if (!_isInvalidAtIndex[i])
                {
                    indexer++;
                }
            }

            throw new System.NullReferenceException("The possible move was not found");
        }

        public bool GetIndex(int index)
        {
            return _isInvalidAtIndex[index];
        }

        public void SetIndex(int index, bool value)
        {
            _isInvalidAtIndex[index] = value;
            if (value)
            {
                _validIndicesCount--;
            }
            else
            {
                _validIndicesCount++;
            }
        }

        public void Reset()
        {
            for (int i = 0; i < _isInvalidAtIndex.Length; i++)
            {
                _isInvalidAtIndex[i] = false;
            }
            _validIndicesCount = _isInvalidAtIndex.Length;
        }
    }
}