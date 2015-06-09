using System;
using Microsoft.SPOT;
using System.Collections;

namespace JasCapture.Form
{
    public class Serie : IEnumerable, ICollection, IList
    {

        public ushort NbOfPoint { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        private ArrayList _plots = new ArrayList();


        private float _maximum;
        public float Maximum
        {
            get
            {
                return _maximum ;
            }
        }

        private float _minimum;
        public float Minimum
        {
            get
            {
                return _minimum;
            }
        }

        private void ComputeStats()
        {
            if (this._plots.Count == 0)
                return;

            _minimum = (float)this._plots[0];
            _maximum = _minimum;
            for (int i = 0; i < this._plots.Count; i++)
            {
                float data = (float)this._plots[i];
                UpdateStats(data);
            }
        }

        private void UpdateStats(float data)
        {
            if (data < _minimum) _minimum = data;
            if (data > _maximum) _maximum = data;
        }

        public IEnumerator GetEnumerator()
        {
            return this._plots.GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            this._plots.CopyTo(array, index);
        }

        public int Count
        {
            get { return this._plots.Count; }
        }

        public bool IsSynchronized
        {
            get { return this._plots.IsSynchronized; }

        }

        public object SyncRoot
        {
            get { return this._plots.SyncRoot; }
        }

        public int Add(object value)
        {
            float data = (float)value;
            this._plots.Add(data);
            if (this._plots.Count > NbOfPoint)
            {
                this._plots.RemoveAt(0);
            }
            // update statistics
            ComputeStats();
            return 1;
        }

        public void Clear()
        {
            this._plots.Clear();
        }

        public bool Contains(object value)
        {
            return this._plots.Contains(value);
        }

        public int IndexOf(object value)
        {
            return this._plots.IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            this._plots.Insert(index, value);
            
            if (this._plots.Count > NbOfPoint)
            {
                this._plots.RemoveAt(0);
            }
           
            UpdateStats((float)value);
           
        }

        public bool IsFixedSize
        {
            get { return true; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Remove(object value)
        {
            this._plots.Remove(value);
            float data = (float)value;
            if (data == _minimum || data == _maximum)
            {
                ComputeStats();
            }
        }

        public void RemoveAt(int index)
        {
            float data = (float)this._plots[index];
            this.RemoveAt(index);
            if (data == _minimum || data == _maximum)
            {
                ComputeStats();
            }
        }

        public object this[int index]
        {
            get
            {
                return this._plots[index];
            }
            set
            {
                this._plots[index] = value;
                ComputeStats();
            }
        }
    }
}
