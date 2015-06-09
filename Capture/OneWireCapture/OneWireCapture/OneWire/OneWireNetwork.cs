using System;
using Microsoft.SPOT.Hardware;
using System.Collections;

namespace OneWireCapture.OneWire
{
    /// <summary>
    /// Represent a OneWire network of sensor
    /// </summary>
    public class OneWireNetwork : IDisposable
    {
        /// <summary>
        /// Define the states of the network
        /// </summary>
        public enum State
        {
            /// <summary>
            /// The network state is not determinated
            /// </summary>
            UNKNOW = 0,
            /// <summary>
            /// The network is ready for accpetiong orders
            /// </summary>
            READY = 1,
            /// <summary>
            /// The network is perfoming reading, writing or send an order
            /// </summary>
            BUSY = 2,
            /// <summary>
            /// The network is set on broadcast mode
            /// </summary>
            BROADCAST = 3,
            /// <summary>
            /// One device is currently selected
            /// </summary>
            SELECTED = 4
        }

        /// <summary>
        /// store in a stack the states of the bus
        /// </summary>
        private Stack _states = new Stack();
        /// <summary>
        /// Store the selected sensor address
        /// </summary>
        private RomAddress _selectedSensor;

        /// <summary>
        /// Get the Selected sensor address
        /// </summary>
        public RomAddress SelectedSensor { get { return _selectedSensor; } }

        /// <summary>
        /// Broadcast order
        /// </summary>
        private const byte BROADCAST_ORDER = 0xCC;
        /// <summary>
        /// Select Order
        /// </summary>
        private const byte SELECT_ORDER = 0x55;

        /// <summary>
        /// OneWire Protocol Provider
        /// </summary>
        private GHIElectronics.NETMF.Hardware.OneWire oneWire;

        /// <summary>
        /// Store the value that indicate if the instance have been disposed
        /// </summary>
        private bool IsDisposed = false;

        /// <summary>
        /// Get the state of the network
        /// </summary>
        public State CurrentState
        {
            get
            {
                if (_states.Count == 0)
                {
                    return State.UNKNOW;
                }
                return (State)_states.Peek();
            }
        }
        /// <summary>
        /// Instanciate an Onwe wire network
        /// </summary>
        /// <param name="pin">Attached pin</param>
        /// <param name="capacity">Number of sesnsors on the bus</param>
        public OneWireNetwork(Cpu.Pin pin)
        {
            this.oneWire = new  GHIElectronics.NETMF.Hardware.OneWire(pin);
            this._states.Push(OneWireNetwork.State.READY);
        }

        /// <summary>
        /// Perform a search on the OneWire bus
        /// </summary>
        public virtual RomAddress[] Discover()
        {
            this._states.Push(State.BUSY);

            ArrayList sensors = new ArrayList();
            Int16 addressLen = 8;
            byte[] romRegistre = new byte[addressLen];

            this.oneWire.Search_Restart();
            while (this.oneWire.Search_GetNextDevice(romRegistre))
            {
                RomAddress address = new RomAddress((byte[])romRegistre.Clone(), addressLen);
                sensors.Add(address);
            }

            this._states.Pop();

            RomAddress[] adresseList = new RomAddress[sensors.Count];
            for (int i = 0; i < adresseList.Length; i++)
            {
                adresseList[i] = (RomAddress)sensors[i];
            }
            return adresseList;
        }

        /// <summary>
        /// Select all sensors on the bus and send an order
        /// <param name="order">Order to send</param>
        /// </summary>
        public virtual void Broadcast()
        {
            this.oneWire.Reset();
            this._states.Push(State.BUSY);
            this.oneWire.WriteByte(BROADCAST_ORDER);
            this._states.Pop();
            this._states.Push(State.BROADCAST);
        }

        /// <summary>
        /// Read data from the network to the buffer
        /// </summary>
        /// <param name="buffer">data buffer for storing </param>
        /// <param name="offset">offset of writing in the buffer</param>
        /// <param name="count">Buffer size</param>
        public virtual void Read(byte[] buffer, int offset, int count)
        {
            this._states.Push(State.BUSY);
            this.oneWire.Read(buffer, offset, count);
            this._states.Pop();
        }

        /// <summary>
        /// Read data from the network
        /// </summary>
        /// <returns>Byte readed</returns>
        public virtual byte Read()
        {
            this._states.Push(State.BUSY);
            byte value = oneWire.ReadByte();
            this._states.Pop();
            return value;
        }

        /// <summary>
        /// Select a device in the network
        /// </summary>
        /// <param name="address">Address of the device</param>
        public virtual void Select(RomAddress address)
        {
            this._states.Push(State.BUSY);
            byte[] bufferAdress = address.ToArray();
            this.oneWire.WriteByte(SELECT_ORDER);
            this.oneWire.Write(bufferAdress, 0, bufferAdress.Length);
            this._states.Pop();

            this._selectedSensor = address;
            this._states.Push(State.SELECTED);
        }

        /// <summary>
        /// Initialize the network.
        /// </summary>
        public virtual void Reset()
        {
            this.oneWire.Reset();
            this._selectedSensor = null;
            this._states.Clear();
            this._states.Push(State.READY);
        }

        /// <summary>
        /// Write data from buffer to the network
        /// </summary>
        /// <param name="buffer">Buffer containing data to write</param>
        /// <param name="offset">Data index</param>
        /// <param name="count">Buffer size</param>
        public void Write(byte[] buffer, int offset, int count)
        {
            this._states.Push(State.BUSY);
            this.oneWire.Write(buffer, offset, count);
            this._states.Pop();
        }

        /// <summary>
        /// Write data from buffer to the network
        /// </summary>
        /// <param name="value">data to write</param>
        public void Write(byte value)
        {
            this._states.Push(State.BUSY);
            this.oneWire.WriteByte(value);
            this._states.Pop();
        }

        /// <summary>
        /// Dispose the instance
        /// </summary>
        /// <param name="dispose">Indicate that the disposing is explicit (not GC)</param>
        protected void Dispose(bool dispose)
        {
            if (dispose)
            {
                this._selectedSensor = null;
                this._states.Clear();
                this.IsDisposed = true;

                this.oneWire.Dispose();
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Dispose the instance of <see cref="OneWireNetwork"/>
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            Dispose(true);
        }
    }
}
