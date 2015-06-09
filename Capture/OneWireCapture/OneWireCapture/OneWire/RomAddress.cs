using System;

namespace OneWireCapture.OneWire
{
    /// <summary>
    /// Represent an ROM Adress of OneWire Device
    /// </summary>
    public class RomAddress
    {
        /// <summary>
        /// Buffer Adresse
        /// </summary>
        private Byte[] adresse;
        /// <summary>
        /// Size of the buffer adress
        /// </summary>
        private Int16 adresseSize;

        /// <summary>
        /// Create a new instance of RomAdresse
        /// </summary>
        /// <param name="romBuffer">Rom buffer</param>
        /// <param name="bufferSize">Size of the buffer</param>
        public RomAddress(byte[] romBuffer, Int16 bufferSize)
        {
            this.adresseSize = bufferSize;
            this.adresse = romBuffer;
        }

        /// <summary>
        /// Copy the buffer adress to an Array of bytes
        ///<returns> ROM Register Adresse</returns>
        /// </summary>
        public byte[] ToArray()
        {
            return (byte[])this.adresse.Clone();
        }

        /// <summary>
        /// Convert the adresse to an String representation
        /// </summary>
        /// <returns>formated ROM Address</returns>
        public override string ToString()
        {
            String text = String.Empty;
            for (int i = 0; i < this.adresse.Length; i++)
            {
                if (i > 0)
                {
                    text += ":";
                }
                text += adresse[i].ToString();
            }
            return text;
        }
    }
}
