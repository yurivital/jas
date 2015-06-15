using System;
using Microsoft.SPOT;
using System.Collections;

namespace JasCapture.Form
{
    /// <summary>
    /// Provide a FIFO data structure for managing a size-fixed array of data
    /// </summary>
    public class Serie : IEnumerable, ICollection, IList
    {
        /// <summary>
        /// Get or set the maximum of data to store
        /// </summtary>
        public ushort NbOfPoint { get; set; }
        /// <summary>
        /// Get or set the value of serie name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// store the collection of data
        /// </summary>
        private ArrayList _plots = new ArrayList();

        /// <summary>
        /// Get the value of the bigger data
        /// </summary>
        public float Maximum
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the value of the smallest data
        /// </summary>
        public float Minimum
        {
            get;
            private set;
        }

        /// <summary>
        /// Compute the Maximum and Minimum data
        /// </summary>
        private void ComputeStats()
        {
            if (this._plots.Count == 0)
                return;

            Minimum = (float)this._plots[0];
            Maximum = Minimum;
            for (int i = 0; i < this._plots.Count; i++)
            {
                float data = (float)this._plots[i];
                UpdateStats(data);
            }
        }

        /// <summary>
        /// Check if the value is between minimum and maximum bounds. If not, update the max or min value.
        /// </summary>
        /// <param name="data">data</param>
        private void UpdateStats(float data)
        {
            if (data < Minimum) Minimum = data;
            if (data > Maximum) Maximum = data;
        }

        /// <summary>
        /// Return the énumerator for the serie datas
        /// </summary>
        /// <returns>Instance of Enumerator</returns>
        public IEnumerator GetEnumerator()
        {
            return this._plots.GetEnumerator();
        }

        /// <summary>
        /// Copie the data to an Array
        /// </summary>
        /// <param name="array">Array to copie to</param>
        /// <param name="index">start index of data to copy </param>
        public void CopyTo(Array array, int index)
        {
            this._plots.CopyTo(array, index);
        }

        /// <summary>
        /// Get the number of data in the Serie
        /// </summary>
        public int Count
        {
            get { return this._plots.Count; }
        }

        /// <summary>
        /// Get the value wich indicate if the Serie is synchronized
        /// </summary>
        public bool IsSynchronized
        {
            get { return this._plots.IsSynchronized; }

        }

        /// <summary>
        /// Get the instance of the synchronization object
        /// </summary>
        public object SyncRoot
        {
            get { return this._plots.SyncRoot; }
        }

        /// <summary>
        /// Add a new data value in the serie. Remove the first value if the serie capacity is reached
        /// </summary>
        /// <param name="value">Data value</param>
        /// <returns>Return the position value of the data in the serie</returns>
        public int Add(object value)
        {
            float data = (float)value;
           int position = this._plots.Add(data);
            if (this._plots.Count > NbOfPoint)
            {
                this._plots.RemoveAt(0);
            }
            // update statistics
            ComputeStats();
            return position;
        }

        /// <summary>
        /// Remove all data in the serie
        /// </summary>
        public void Clear()
        {
            this._plots.Clear();
        }

        /// <summary>
        /// Seach for the instance of data in the serie 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(object value)
        {
            return this._plots.Contains(value);
        }

        /// <summary>
        /// Return of the postion in the serie of data instance
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int IndexOf(object value)
        {
            return this._plots.IndexOf(value);
        }

        /// <summary>
        /// Insert a new data value in the serie. Remove the first value if the serie capacity is reached
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void Insert(int index, object value)
        {
            this._plots.Insert(index, value);
            
            if (this._plots.Count > NbOfPoint)
            {
                this._plots.RemoveAt(0);
            }
           
            UpdateStats((float)value);
        }

        /// <summary>
        /// Return always True. Serie is a size-fixes buffer. <seealso cref="Serie"/>
        /// </summary>
        public bool IsFixedSize
        {
            get { return true; }
        }

        /// <summary>
        /// Return false. Serie is Read-Write capable buffer.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Remove an instance of value in the serie
        /// </summary>
        /// <param name="value">value to remove</param>
        public void Remove(object value)
        {
            this._plots.Remove(value);
            float data = (float)value;
            if (data == Minimum || data == Maximum)
            {
                ComputeStats();
            }
        }

        /// <summary>
        /// Remove a data value determined by his positions
        /// </summary>
        /// <param name="index">Position of the value to remove</param>
        public void RemoveAt(int index)
        {
            float data = (float)this._plots[index];
            this.RemoveAt(index);
            if (data == Minimum || data == Maximum)
            {
                ComputeStats();
            }
        }

        /// <summary>
        /// Get or set the 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
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
