/*
Copyright 2011 GHI Electronics LLC
Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
http://www.apache.org/licenses/LICENSE-2.0
Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License. 
*/

using System;
using System.Threading;
using GHIElectronics.NETMF.Hardware;
using Microsoft.SPOT.Hardware;

namespace GHIElectronics.NETMF.FEZ
{
    /// <summary>
    /// Vendor specific library
    /// </summary>
    public static partial class FEZ_Components
    {
        /// <summary>
        /// Manage a touch LCD screen
        /// </summary>
        public class FEZTouch : IDisposable, JasCapture.Form.IDrawer
        {
            /// <summary>
            /// Configuration for the LCD screen
            /// </summary>
            public class LCDConfiguration
            {
                /// <summary>
                /// Data wiring
                /// </summary>
                internal Cpu.Pin[] dataPins;
                /// <summary>
                /// Write operation pin
                /// </summary>
                internal Cpu.Pin writePin;
                /// <summary>
                /// Read operation pin
                /// </summary>
                internal Cpu.Pin readPin;
                /// <summary>
                /// device select pin
                /// </summary>
                internal Cpu.Pin chipSelect;
                /// <summary>
                /// Reset pin
                /// </summary>
                internal Cpu.Pin reset;
                /// <summary>
                /// 
                /// </summary>
                internal Cpu.Pin RS;
                /// <summary>
                /// backlight intensity control pin
                /// </summary>
                internal Cpu.Pin backLight;

                /// <summary>
                /// Create a new instance of <see cref="LCDConfiguration"/>
                /// </summary>
                /// <param name="reset"></param>
                /// <param name="chipSelect"></param>
                /// <param name="RS"></param>
                /// <param name="backLight"></param>
                /// <param name="dataPins"></param>
                /// <param name="writePin"></param>
                /// <param name="readPin"></param>
                public LCDConfiguration(FEZ_Pin.Digital reset, FEZ_Pin.Digital chipSelect, FEZ_Pin.Digital RS, FEZ_Pin.Digital backLight, FEZ_Pin.Digital[] dataPins, FEZ_Pin.Digital writePin, FEZ_Pin.Digital readPin)
                {
                    this.dataPins = new Cpu.Pin[8];
                    for (int i = 0; i < 8; i++)
                        this.dataPins[i] = (Cpu.Pin)dataPins[i];

                    this.writePin = (Cpu.Pin)writePin;
                    this.readPin = (Cpu.Pin)readPin;
                    this.chipSelect = (Cpu.Pin)chipSelect;
                    this.reset = (Cpu.Pin)reset;
                    this.RS = (Cpu.Pin)RS;
                    this.backLight = (Cpu.Pin)backLight;
                }
            }

            /// <summary>
            /// Embed the configuratio for touch capability
            /// </summary>
            public class TouchConfiguration
            {
                internal SPI.SPI_module channel;
                internal Cpu.Pin chipSelect;
                internal Cpu.Pin touchIRQ;

                public TouchConfiguration(SPI.SPI_module channel, FEZ_Pin.Digital chipSelect, FEZ_Pin.Digital touchIRQ)
                {
                    this.channel = channel;
                    this.chipSelect = (Cpu.Pin)chipSelect;
                    this.touchIRQ = (Cpu.Pin)touchIRQ;
                }
            }

            public const int ScreenWidth = 240;
            public const int ScreenHeight = 320;

            private bool disposed = false;
            private const int BUFFERING_SIZE = 1024;
            private byte[] buffer = new byte[BUFFERING_SIZE];
            private byte[] regBuffer = new byte[2];

            private ParallelPort pp;
            private OutputPort lcdReset;
            private OutputPort lcdCS;
            private OutputPort lcdRS;
            private OutputCompare backLight;

            // Touch
            SPI spi;
            InputPort touchIRQ;
            const int TOUCH_SAMPLING_TIME = 10;
            bool terminateTouchThread;
            Thread touchThread;

            private const byte REGISTER_WRITE_GRAM = 0x22;

            public FEZTouch(LCDConfiguration lcdConfig)
            {
                InitLCD(lcdConfig);
            }

            public FEZTouch(LCDConfiguration lcdConfig, TouchConfiguration touchConfig)
            {
                InitLCD(lcdConfig);
                InitTouch(touchConfig);
            }

            private void InitTouch(TouchConfiguration touchConfig)
            {
                spi = new SPI(new SPI.Configuration(touchConfig.chipSelect, false, 1, 1, false, true, 2000, touchConfig.channel));
                touchIRQ = new InputPort(touchConfig.touchIRQ, false, Port.ResistorMode.Disabled);
                terminateTouchThread = false;
                touchThread = new Thread(TouchThread);
                touchThread.Priority = ThreadPriority.Highest;
                touchThread.Start();
            }

            public delegate void TouchEventHandler(int x, int y);

            public event TouchEventHandler TouchDownEvent = delegate { };
            public event TouchEventHandler TouchUpEvent = delegate { };
            public event TouchEventHandler TouchMoveEvent = delegate { };

            void TouchThread()
            {
                int lastX = 0;
                int lastY = 0;
                bool lastStatus = false; // true means there are touches

                int x;
                int y;
                bool status;

                byte[] writeBuffer = new byte[] { 0, 0, 0, 0 };
                byte[] readBuffer = new byte[2];

                while (!terminateTouchThread)
                {
                    Thread.Sleep(TOUCH_SAMPLING_TIME);

                    status = !touchIRQ.Read();

                    if (status)
                    {
                        writeBuffer[0] = 0x90;
                        spi.WriteRead(writeBuffer, readBuffer, 1);
                        y = readBuffer[0];
                        y <<= 8;
                        y |= readBuffer[1];
                        y >>= 3;

                        writeBuffer[0] = 0xD0;
                        spi.WriteRead(writeBuffer, readBuffer, 1);
                        x = readBuffer[0];
                        x <<= 8;
                        x |= readBuffer[1];
                        x >>= 3;

                        // Calibrate 
                        if (x > 3750)
                            x = 3750;
                        else if (x < 280)
                            x = 280;

                        if (y > 3850)
                            y = 3850;
                        else if (y < 450)
                            y = 450;

                        x = (3750 - x) * (240 - 1) / (3750 - 280);
                        y = (3850 - y) * (320 - 1) / (3850 - 450);

                        if (lastStatus == false)
                        {
                            TouchDownEvent(x, y);

                            lastStatus = true;
                            lastX = x;
                            lastY = y;
                        }
                        else
                        {
                            // filter small changes
                            if (global::System.Math.Abs(x - lastX) > 5 ||
                                global::System.Math.Abs(y - lastY) > 5
                                )
                            {
                                TouchMoveEvent(x, y);
                                lastX = x;
                                lastY = y;
                            }
                        }
                    }
                    else
                    {
                        if (lastStatus == true)
                        {
                            TouchUpEvent(lastX, lastY);
                            lastStatus = false;
                        }
                    }
                }
            }

            public virtual void SetPixel(int x, int y, Color col)
            {
                if (x < 0 || y < 0 || x >= ScreenWidth || y >= ScreenHeight)
                    throw new ArgumentException();

                lcdCS.Write(false);

                buffer[0] = (byte)((int)col >> 8);
                buffer[1] = (byte)(col);

                SetDrawingWindow(x, y, 1, 1);

                SetRegister(REGISTER_WRITE_GRAM);
                pp.Write(buffer, 0, 2);

                lcdCS.Write(true);
            }

            private void Swap(ref int a1, ref int a2)
            {
                int temp = a1;
                a1 = a2;
                a2 = temp;
            }

            public virtual void DrawLine(int x0, int y0, int x1, int y1, Color col)
            {
                if (x0 < 0 || y0 < 0 || x0 >= ScreenWidth || y0 >= ScreenHeight)
                    throw new ArgumentException();

                if (x1 < 0 || y1 < 0 || x1 >= ScreenWidth || y1 >= ScreenHeight)
                    throw new ArgumentException();

                lcdCS.Write(false);

                buffer[0] = (byte)((int)col >> 8);
                buffer[1] = (byte)(col);

                int dy = y1 - y0;
                int dx = x1 - x0;

                float m = 0;
                int b = 0;

                if (dx != 0)
                {
                    m = ((float)(dy)) / (dx);
                    b = y0 - (int)(m * x0);
                }

                SetDrawingWindow(0, 0, ScreenWidth, ScreenHeight);

                if (global::System.Math.Abs(dx) >= global::System.Math.Abs(dy))
                {
                    if (x0 > x1)
                    {
                        Swap(ref x0, ref x1);
                        Swap(ref y0, ref y1);
                    }

                    while (x0 <= x1)
                    {
                        SetPixelAddress(x0, y0);

                        SetRegister(REGISTER_WRITE_GRAM);
                        pp.Write(buffer, 0, 2);

                        x0++;

                        if (x0 <= x1)
                        {
                            y0 = (int)(m * x0) + b;
                        }
                    }
                }
                else
                {
                    if (y0 > y1)
                    {
                        Swap(ref x0, ref x1);
                        Swap(ref y0, ref y1);
                    }

                    while (y0 <= y1)
                    {
                        SetPixelAddress(x0, y0);

                        SetRegister(REGISTER_WRITE_GRAM);
                        pp.Write(buffer, 0, 2);

                        y0++;

                        if (y0 <= y1)
                        {
                            if (dx != 0)
                                x0 = (int)((float)(y0 - b) / m);
                        }
                    }
                }

                lcdCS.Write(true);
            }

            public virtual void FillRectangle(int x, int y, int width, int height, Color col)
            {
                if (x < 0 || y < 0 || (x + width) > ScreenWidth || (y + height) > ScreenHeight)
                    throw new ArgumentException();

                lcdCS.Write(false);

                int pixelCount = width * height;
                int bufferPixels = buffer.Length / 2; // every pixel is 2 bytes
                byte h = (byte)((int)col >> 8);
                byte l = (byte)(col);

                // fill buffer
                for (int i = 0; i < buffer.Length; i = i + 2)
                {
                    buffer[i] = h;
                    buffer[i + 1] = l;
                }

                SetDrawingWindow(x, y, width, height);

                SetRegister(REGISTER_WRITE_GRAM);

                int loops = pixelCount / bufferPixels;

                for (int i = 0; i < loops; i++)
                {
                    pp.Write(buffer, 0, buffer.Length);
                }

                int pixelsLeft = pixelCount % bufferPixels;
                if (pixelsLeft > 0)
                {
                    // every pixel is 2 bytes
                    pp.Write(buffer, 0, pixelsLeft * 2);
                }

                lcdCS.Write(true);
            }

            public virtual void DrawImage(int x, int y, Image image)
            {
                if (x < 0 || y < 0 || (x + image.Width) > ScreenWidth || (y + image.Height) > ScreenHeight)
                    throw new ArgumentException();

                lcdCS.Write(false);

                SetDrawingWindow(x, y, image.Width, image.Height);

                SetRegister(REGISTER_WRITE_GRAM);

                pp.Write(image.imgBytes, Image.IMG_PIXELS_INDEX, image.imgBytes.Length - Image.IMG_PIXELS_INDEX);

                lcdCS.Write(true);
            }

            public enum Color : ushort
            {
                White = 0xFFFF,
                Black = 0x0000,
                Red = (255 >> 3) | ((0 & 0xFC) << 3) | ((0 & 0xF8) << 8),
                Blue = (0 >> 3) | ((0 & 0xFC) << 3) | ((255 & 0xF8) << 8),
                Green = (0 >> 3) | ((255 & 0xFC) << 3) | ((0 & 0xF8) << 8),
                Cyan = (0 >> 3) | ((255 & 0xFC) << 3) | ((255 & 0xF8) << 8),
                Gray = (0x80 >> 3) | ((0x80 & 0xFC) << 3) | ((0x80 & 0xF8) << 8),
                Magneta = (255 >> 3) | ((0 & 0xFC) << 3) | ((255 & 0xF8) << 8),
                Yellow = (255 >> 3) | ((255 & 0xFC) << 3) | ((0 & 0xF8) << 8),
            }

            public Color ColorFromRGB(byte red, byte green, byte blue)
            {
                return (Color)((red >> 3) | ((green & 0xFC) << 3) | ((blue & 0xF8) << 8));
            }

            public class Image
            {
                private const uint SIGNATURE = 0x354A82B8;
                internal const int IMG_PIXELS_INDEX = 8;
                internal byte[] imgBytes;

                public readonly int Width;
                public readonly int Height;

                public Image(byte[] imgBytes)
                {
                    if (Utility.ExtractValueFromArray(imgBytes, 0, 4) != SIGNATURE)
                        throw new ArgumentException();

                    int width = (int)Utility.ExtractValueFromArray(imgBytes, 4, 2);
                    int height = (int)Utility.ExtractValueFromArray(imgBytes, 6, 2);

                    if (width * height * 2 + 8 != imgBytes.Length)
                        throw new ArgumentException();

                    this.imgBytes = imgBytes;
                    this.Width = width;
                    this.Height = height;
                }
            }

            private void InitLCD(LCDConfiguration lcdConfig)
            {
                pp = new ParallelPort(lcdConfig.dataPins, lcdConfig.writePin, lcdConfig.readPin);
                lcdReset = new OutputPort(lcdConfig.reset, true);
                lcdCS = new OutputPort(lcdConfig.chipSelect, true);
                lcdRS = new OutputPort(lcdConfig.RS, true);
                backLight = new OutputCompare(lcdConfig.backLight, true, 2);

                lcdReset.Write(true);
                Thread.Sleep(5);
                lcdReset.Write(false);
                Thread.Sleep(5);
                lcdReset.Write(true);
                Thread.Sleep(5);

                lcdCS.Write(false);

                //************* Start Initial Sequence **********//
                WriteRegister(0x0001, 0x0100); // set SS and SM bit
                WriteRegister(0x0002, 0x0200); // set 1 line inversion
                WriteRegister(0x0003, 0x0030); // set GRAM write direction and BGR=0.
                WriteRegister(0x0004, 0x0000); // Resize register
                WriteRegister(0x0008, 0x0207); // set the back porch and front porch
                WriteRegister(0x0009, 0x0000); // set non-display area refresh cycle ISC[3:0]
                WriteRegister(0x000A, 0x0000); // FMARK function
                WriteRegister(0x000C, 0x0000); // RGB interface setting
                WriteRegister(0x000D, 0x0000); // Frame marker Position
                WriteRegister(0x000F, 0x0000); // RGB interface polarity
                //*************Power On sequence ****************//
                WriteRegister(0x0010, 0x0000); // SAP, BT[3:0], AP, DSTB, SLP, STB
                WriteRegister(0x0011, 0x0007); // DC1[2:0], DC0[2:0], VC[2:0]
                WriteRegister(0x0012, 0x0000); // VREG1OUT voltage
                WriteRegister(0x0013, 0x0000); // VDV[4:0] for VCOM amplitude
                WriteRegister(0x0007, 0x0001);
                Thread.Sleep(200); // Dis-charge capacitor power voltage
                WriteRegister(0x0010, 0x1690); // SAP, BT[3:0], AP, DSTB, SLP, STB
                WriteRegister(0x0011, 0x0227); // Set DC1[2:0], DC0[2:0], VC[2:0]
                Thread.Sleep(50); // Delay 50ms
                WriteRegister(0x0012, 0x000D); // 0012
                Thread.Sleep(50); // Delay 50ms
                WriteRegister(0x0013, 0x1200); // VDV[4:0] for VCOM amplitude
                WriteRegister(0x0029, 0x000A); // 04  VCM[5:0] for VCOMH
                WriteRegister(0x002B, 0x000D); // Set Frame Rate
                Thread.Sleep(50); // Delay 50ms
                WriteRegister(0x0020, 0x0000); // GRAM horizontal Address
                WriteRegister(0x0021, 0x0000); // GRAM Vertical Address
                // ----------- Adjust the Gamma Curve ----------//
                WriteRegister(0x0030, 0x0000);
                WriteRegister(0x0031, 0x0404);
                WriteRegister(0x0032, 0x0003);
                WriteRegister(0x0035, 0x0405);
                WriteRegister(0x0036, 0x0808);
                WriteRegister(0x0037, 0x0407);
                WriteRegister(0x0038, 0x0303);
                WriteRegister(0x0039, 0x0707);
                WriteRegister(0x003C, 0x0504);
                WriteRegister(0x003D, 0x0808);
                //------------------ Set GRAM area ---------------//
                WriteRegister(0x0050, 0x0000); // Horizontal GRAM Start Address
                WriteRegister(0x0051, 0x00EF); // Horizontal GRAM End Address
                WriteRegister(0x0052, 0x0000); // Vertical GRAM Start Address
                WriteRegister(0x0053, 0x013F); // Vertical GRAM Start Address
                WriteRegister(0x0060, 0xA700); // Gate Scan Line
                WriteRegister(0x0061, 0x0001); // NDL,VLE, REV

                WriteRegister(0x006A, 0x0000); // set scrolling line
                //-------------- Partial Display Control ---------//
                WriteRegister(0x0080, 0x0000);
                WriteRegister(0x0081, 0x0000);
                WriteRegister(0x0082, 0x0000);
                WriteRegister(0x0083, 0x0000);
                WriteRegister(0x0084, 0x0000);
                WriteRegister(0x0085, 0x0000);
                //-------------- Panel Control -------------------//
                WriteRegister(0x0090, 0x0010);
                WriteRegister(0x0092, 0x0000);
                WriteRegister(0x0007, 0x0133); // 262K color and display ON

                lcdCS.Write(true);
            }

            ~FEZTouch()
            {
                Dispose();
            }

            public void Dispose()
            {
                if (!disposed)
                {
                    disposed = true;

                    if (spi != null)
                    {
                        terminateTouchThread = true;
                        touchThread.Join();

                        spi.Dispose();
                        touchIRQ.Dispose();
                    }

                    TouchDownEvent = null;
                    TouchUpEvent = null;
                    TouchMoveEvent = null;

                    pp.Dispose();
                    lcdReset.Dispose();
                    lcdCS.Dispose();
                    lcdRS.Dispose();
                    backLight.Dispose();
                }
            }

            private void SetRegister(byte reg)
            {
                lcdRS.Write(false);

                regBuffer[0] = 0;
                regBuffer[1] = reg;
                pp.Write(regBuffer, 0, 2);

                lcdRS.Write(true);
            }

            private void WriteRegister(byte reg, ushort value)
            {
                SetRegister(reg);

                regBuffer[0] = (byte)(value >> 8);
                regBuffer[1] = (byte)(value);
                pp.Write(regBuffer, 0, 2);
            }

            private void SetPixelAddress(int x, int y)
            {
                WriteRegister(0x20, (ushort)x);
                WriteRegister(0x21, (ushort)y);
            }

            private void SetDrawingWindow(int x, int y, int width, int height)
            {
                // pixel address
                SetPixelAddress(x, y);

                // window
                WriteRegister(0x50, (ushort)x);
                WriteRegister(0x52, (ushort)y);
                WriteRegister(0x51, (ushort)(x + width - 1));
                WriteRegister(0x53, (ushort)(y + height - 1));
            }

            public virtual void SetBrightness(int percentage)
            {
                if (percentage < 0 || percentage > 100)
                    throw new ArgumentException();

                if (percentage == 0)
                    backLight.Set(false);
                else if (percentage == 100)
                    backLight.Set(true);
                else
                {
                    const int PERIOD = 10000;
                    int highTime = PERIOD * percentage / 100;
                    backLight.Set(true, new uint[2] { (uint)highTime, (uint)(PERIOD - highTime) }, 0, 2, true);
                }
            }

            public virtual void DrawString(string str, int x, int y, Color foreColor, Color backColor)
            {
                if (x < 0 || y < 0 || (x + str.Length * FONT_WIDTH) > ScreenWidth || (y + FONT_HEIGHT) > ScreenHeight)
                    throw new ArgumentException();

                int index = 0;
                int length;

                // first four bytes are color in the buffer
                buffer[0] = (byte)((int)foreColor >> 8);
                buffer[1] = (byte)(foreColor);

                buffer[2] = (byte)((int)backColor >> 8);
                buffer[3] = (byte)(backColor);

                while (index < str.Length)
                {
                    length = (str.Length - index) > CHARS_PER_BUFFER ? CHARS_PER_BUFFER : (str.Length - index);
                    DrawString_Internal(x, y, str.Substring(index, length));
                    index += length;
                    x += length * FONT_WIDTH;
                }
            }

            private void DrawString_Internal(int x, int y, string val)
            {
                lcdCS.Write(false);

                byte ch;

                // get the foreground and background color bytes before any loops 
                byte bc0 = buffer[2];
                byte bc1 = buffer[3];
                byte fc0 = buffer[0];
                byte fc1 = buffer[1];

                // first four bytes are used for color
                int bufferIndex = 4;

                SetDrawingWindow(x, y, FONT_WIDTH * val.Length, FONT_HEIGHT);

                for (int j = 0; j < 12; j++)
                {
                    for (int currentChar = 0; currentChar < val.Length; currentChar++)
                    {
                        ch = font[((val[currentChar] - 32) * 12) + j];

                        // use defined masks

                        if ((ch & 0x80) != 0) { buffer[bufferIndex] = fc0; buffer[bufferIndex + 1] = fc1; }
                        else { buffer[bufferIndex] = bc0; buffer[bufferIndex + 1] = bc1; }

                        if ((ch & 0x40) != 0) { buffer[bufferIndex + 2] = fc0; buffer[bufferIndex + 3] = fc1; }
                        else { buffer[bufferIndex + 2] = bc0; buffer[bufferIndex + 3] = bc1; }

                        if ((ch & 0x20) != 0) { buffer[bufferIndex + 4] = fc0; buffer[bufferIndex + 5] = fc1; }
                        else { buffer[bufferIndex + 4] = bc0; buffer[bufferIndex + 5] = bc1; }

                        if ((ch & 0x10) != 0) { buffer[bufferIndex + 6] = fc0; buffer[bufferIndex + 7] = fc1; }
                        else { buffer[bufferIndex + 6] = bc0; buffer[bufferIndex + 7] = bc1; }

                        if ((ch & 0x8) != 0) { buffer[bufferIndex + 8] = fc0; buffer[bufferIndex + 9] = fc1; }
                        else { buffer[bufferIndex + 8] = bc0; buffer[bufferIndex + 9] = bc1; }

                        if ((ch & 0x04) != 0) { buffer[bufferIndex + 10] = fc0; buffer[bufferIndex + 11] = fc1; }
                        else { buffer[bufferIndex + 10] = bc0; buffer[bufferIndex + 11] = bc1; }

                        if ((ch & 0x02) != 0) { buffer[bufferIndex + 12] = fc0; buffer[bufferIndex + 13] = fc1; }
                        else { buffer[bufferIndex + 12] = bc0; buffer[bufferIndex + 13] = bc1; }

                        if ((ch & 0x01) != 0) { buffer[bufferIndex + 14] = fc0; buffer[bufferIndex + 15] = fc1; }
                        else { buffer[bufferIndex + 14] = bc0; buffer[bufferIndex + 15] = bc1; }
                        bufferIndex += 16;
                    }
                }

                SetRegister(REGISTER_WRITE_GRAM);
                pp.Write(buffer, 4, bufferIndex - 4);

                lcdCS.Write(true);
            }

            public const int FONT_WIDTH = 8;
            public const int FONT_HEIGHT = 12;

            // we are using the first four bytes for forecolor and backcolor
            private const int CHARS_PER_BUFFER = BUFFERING_SIZE / (FONT_WIDTH * FONT_HEIGHT * 2 - 4);
            #region FONT
            static private byte[] font = new byte[]{         
            /*--  show:     --*/
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            /*--  show:  !  --*/
            0x00,0x00,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x20,0x00,0x00,
            /*--  show:  "  --*/
            0x00,0x28,0x50,0x50,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            /*--  show:  #  --*/
            0x00,0x00,0x28,0x28,0xFC,0x28,0x50,0xFC,0x50,0x50,0x00,0x00,
            /*--  show:  $  --*/
            0x00,0x20,0x78,0xA8,0xA0,0x60,0x30,0x28,0xA8,0xF0,0x20,0x00,
            /*--  show:  %  --*/
            0x00,0x00,0x48,0xA8,0xB0,0x50,0x28,0x34,0x54,0x48,0x00,0x00,
            /*--  show:  &  --*/
            0x00,0x00,0x20,0x50,0x50,0x78,0xA8,0xA8,0x90,0x6C,0x00,0x00,
            /*--  show:  '  --*/
            0x00,0x40,0x40,0x80,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            /*--  show:  (  --*/
            0x00,0x04,0x08,0x10,0x10,0x10,0x10,0x10,0x10,0x08,0x04,0x00,
            /*--  show:  )  --*/
            0x00,0x40,0x20,0x10,0x10,0x10,0x10,0x10,0x10,0x20,0x40,0x00,
            /*--  show:  *  --*/
            0x00,0x00,0x00,0x20,0xA8,0x70,0x70,0xA8,0x20,0x00,0x00,0x00,
            /*--  show:  +  --*/
            0x00,0x00,0x20,0x20,0x20,0xF8,0x20,0x20,0x20,0x00,0x00,0x00,
            /*--  show:  ,  --*/
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x40,0x40,0x80,
            /*--  show:  -  --*/
            0x00,0x00,0x00,0x00,0x00,0xF8,0x00,0x00,0x00,0x00,0x00,0x00,
            /*--  show:  .  --*/
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x40,0x00,0x00,
            /*--  show:  /  --*/
            0x00,0x08,0x10,0x10,0x10,0x20,0x20,0x40,0x40,0x40,0x80,0x00,
            /*--  show:  0  --*/
            0x00,0x00,0x70,0x88,0x88,0x88,0x88,0x88,0x88,0x70,0x00,0x00,
            /*--  show:  1  --*/
            0x00,0x00,0x20,0x60,0x20,0x20,0x20,0x20,0x20,0x70,0x00,0x00,
            /*--  show:  2  --*/
            0x00,0x00,0x70,0x88,0x88,0x10,0x20,0x40,0x80,0xF8,0x00,0x00,
            /*--  show:  3  --*/
            0x00,0x00,0x70,0x88,0x08,0x30,0x08,0x08,0x88,0x70,0x00,0x00,
            /*--  show:  4  --*/
            0x00,0x00,0x10,0x30,0x50,0x50,0x90,0x78,0x10,0x18,0x00,0x00,
            /*--  show:  5  --*/
            0x00,0x00,0xF8,0x80,0x80,0xF0,0x08,0x08,0x88,0x70,0x00,0x00,
            /*--  show:  6  --*/
            0x00,0x00,0x70,0x90,0x80,0xF0,0x88,0x88,0x88,0x70,0x00,0x00,
            /*--  show:  7  --*/
            0x00,0x00,0xF8,0x90,0x10,0x20,0x20,0x20,0x20,0x20,0x00,0x00,
            /*--  show:  8  --*/
            0x00,0x00,0x70,0x88,0x88,0x70,0x88,0x88,0x88,0x70,0x00,0x00,
            /*--  show:  9  --*/
            0x00,0x00,0x70,0x88,0x88,0x88,0x78,0x08,0x48,0x70,0x00,0x00,
            /*--  show:  :  --*/
            0x00,0x00,0x00,0x00,0x20,0x00,0x00,0x00,0x00,0x20,0x00,0x00,
            /*--  show:  ;  --*/
            0x00,0x00,0x00,0x00,0x00,0x20,0x00,0x00,0x00,0x20,0x20,0x00,
            /*--  show:  <  --*/
            0x00,0x04,0x08,0x10,0x20,0x40,0x20,0x10,0x08,0x04,0x00,0x00,
            /*--  show:  =  --*/
            0x00,0x00,0x00,0x00,0xF8,0x00,0x00,0xF8,0x00,0x00,0x00,0x00,
            /*--  show:  >  --*/
            0x00,0x40,0x20,0x10,0x08,0x04,0x08,0x10,0x20,0x40,0x00,0x00,
            /*--  show:  ?  --*/
            0x00,0x00,0x70,0x88,0x88,0x10,0x20,0x20,0x00,0x20,0x00,0x00,
            /*--  show:  @  --*/
            0x00,0x00,0x70,0x88,0x98,0xA8,0xA8,0xB8,0x80,0x78,0x00,0x00,
            /*--  show:  A  --*/
            0x00,0x00,0x20,0x20,0x30,0x50,0x50,0x78,0x48,0xCC,0x00,0x00,
            /*--  show:  B  --*/
            0x00,0x00,0xF0,0x48,0x48,0x70,0x48,0x48,0x48,0xF0,0x00,0x00,
            /*--  show:  C  --*/
            0x00,0x00,0x78,0x88,0x80,0x80,0x80,0x80,0x88,0x70,0x00,0x00,
            /*--  show:  D  --*/
            0x00,0x00,0xF0,0x48,0x48,0x48,0x48,0x48,0x48,0xF0,0x00,0x00,
            /*--  show:  E  --*/
            0x00,0x00,0xF8,0x48,0x50,0x70,0x50,0x40,0x48,0xF8,0x00,0x00,
            /*--  show:  F  --*/
            0x00,0x00,0xF8,0x48,0x50,0x70,0x50,0x40,0x40,0xE0,0x00,0x00,
            /*--  show:  G  --*/
            0x00,0x00,0x38,0x48,0x80,0x80,0x9C,0x88,0x48,0x30,0x00,0x00,
            /*--  show:  H  --*/
            0x00,0x00,0xCC,0x48,0x48,0x78,0x48,0x48,0x48,0xCC,0x00,0x00,
            /*--  show:  I  --*/
            0x00,0x00,0xF8,0x20,0x20,0x20,0x20,0x20,0x20,0xF8,0x00,0x00,
            /*--  show:  J  --*/
            0x00,0x00,0x7C,0x10,0x10,0x10,0x10,0x10,0x10,0x90,0xE0,0x00,
            /*--  show:  K  --*/
            0x00,0x00,0xEC,0x48,0x50,0x60,0x50,0x50,0x48,0xEC,0x00,0x00,
            /*--  show:  L  --*/
            0x00,0x00,0xE0,0x40,0x40,0x40,0x40,0x40,0x44,0xFC,0x00,0x00,
            /*--  show:  M  --*/
            0x00,0x00,0xD8,0xD8,0xD8,0xD8,0xA8,0xA8,0xA8,0xA8,0x00,0x00,
            /*--  show:  N  --*/
            0x00,0x00,0xDC,0x48,0x68,0x68,0x58,0x58,0x48,0xE8,0x00,0x00,
            /*--  show:  O  --*/
            0x00,0x00,0x70,0x88,0x88,0x88,0x88,0x88,0x88,0x70,0x00,0x00,
            /*--  show:  P  --*/
            0x00,0x00,0xF0,0x48,0x48,0x70,0x40,0x40,0x40,0xE0,0x00,0x00,
            /*--  show:  Q  --*/
            0x00,0x00,0x70,0x88,0x88,0x88,0x88,0xE8,0x98,0x70,0x18,0x00,
            /*--  show:  R  --*/
            0x00,0x00,0xF0,0x48,0x48,0x70,0x50,0x48,0x48,0xEC,0x00,0x00,
            /*--  show:  S  --*/
            0x00,0x00,0x78,0x88,0x80,0x60,0x10,0x08,0x88,0xF0,0x00,0x00,
            /*--  show:  T  --*/
            0x00,0x00,0xF8,0xA8,0x20,0x20,0x20,0x20,0x20,0x70,0x00,0x00,
            /*--  show:  U  --*/
            0x00,0x00,0xCC,0x48,0x48,0x48,0x48,0x48,0x48,0x30,0x00,0x00,
            /*--  show:  V  --*/
            0x00,0x00,0xCC,0x48,0x48,0x50,0x50,0x30,0x20,0x20,0x00,0x00,
            /*--  show:  W  --*/
            0x00,0x00,0xA8,0xA8,0xA8,0x70,0x50,0x50,0x50,0x50,0x00,0x00,
            /*--  show:  X  --*/
            0x00,0x00,0xD8,0x50,0x50,0x20,0x20,0x50,0x50,0xD8,0x00,0x00,
            /*--  show:  Y  --*/
            0x00,0x00,0xD8,0x50,0x50,0x20,0x20,0x20,0x20,0x70,0x00,0x00,
            /*--  show:  Z  --*/
            0x00,0x00,0xF8,0x90,0x10,0x20,0x20,0x40,0x48,0xF8,0x00,0x00,
            /*--  show:  [  --*/
            0x00,0x38,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x38,0x00,
            /*--  show:  \  --*/
            0x00,0x40,0x40,0x40,0x20,0x20,0x10,0x10,0x10,0x08,0x00,0x00,
            /*--  show:  ]  --*/
            0x00,0x70,0x10,0x10,0x10,0x10,0x10,0x10,0x10,0x10,0x70,0x00,
            /*--  show:  ^  --*/
            0x00,0x20,0x50,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            /*--  show:  _  --*/
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xFC,
            /*--  show:  `  --*/
            0x00,0x20,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            /*--  show:  a  --*/
            0x00,0x00,0x00,0x00,0x00,0x30,0x48,0x38,0x48,0x3C,0x00,0x00,
            /*--  show:  b  --*/
            0x00,0x00,0xC0,0x40,0x40,0x70,0x48,0x48,0x48,0x70,0x00,0x00,
            /*--  show:  c  --*/
            0x00,0x00,0x00,0x00,0x00,0x38,0x48,0x40,0x40,0x38,0x00,0x00,
            /*--  show:  d  --*/
            0x00,0x00,0x18,0x08,0x08,0x38,0x48,0x48,0x48,0x3C,0x00,0x00,
            /*--  show:  e  --*/
            0x00,0x00,0x00,0x00,0x00,0x30,0x48,0x78,0x40,0x38,0x00,0x00,
            /*--  show:  f  --*/
            0x00,0x00,0x1C,0x20,0x20,0x78,0x20,0x20,0x20,0x78,0x00,0x00,
            /*--  show:  g  --*/
            0x00,0x00,0x00,0x00,0x00,0x3C,0x48,0x30,0x40,0x78,0x44,0x38,
            /*--  show:  h  --*/
            0x00,0x00,0xC0,0x40,0x40,0x70,0x48,0x48,0x48,0xEC,0x00,0x00,
            /*--  show:  i  --*/
            0x00,0x00,0x20,0x00,0x00,0x60,0x20,0x20,0x20,0x70,0x00,0x00,
            /*--  show:  j  --*/
            0x00,0x00,0x10,0x00,0x00,0x30,0x10,0x10,0x10,0x10,0x10,0xE0,
            /*--  show:  k  --*/
            0x00,0x00,0xC0,0x40,0x40,0x5C,0x50,0x70,0x48,0xEC,0x00,0x00,
            /*--  show:  l  --*/
            0x00,0x00,0xE0,0x20,0x20,0x20,0x20,0x20,0x20,0xF8,0x00,0x00,
            /*--  show:  m  --*/
            0x00,0x00,0x00,0x00,0x00,0xF0,0xA8,0xA8,0xA8,0xA8,0x00,0x00,
            /*--  show:  n  --*/
            0x00,0x00,0x00,0x00,0x00,0xF0,0x48,0x48,0x48,0xEC,0x00,0x00,
            /*--  show:  o  --*/
            0x00,0x00,0x00,0x00,0x00,0x30,0x48,0x48,0x48,0x30,0x00,0x00,
            /*--  show:  p  --*/
            0x00,0x00,0x00,0x00,0x00,0xF0,0x48,0x48,0x48,0x70,0x40,0xE0,
            /*--  show:  q  --*/
            0x00,0x00,0x00,0x00,0x00,0x38,0x48,0x48,0x48,0x38,0x08,0x1C,
            /*--  show:  r  --*/
            0x00,0x00,0x00,0x00,0x00,0xD8,0x60,0x40,0x40,0xE0,0x00,0x00,
            /*--  show:  s  --*/
            0x00,0x00,0x00,0x00,0x00,0x78,0x40,0x30,0x08,0x78,0x00,0x00,
            /*--  show:  t  --*/
            0x00,0x00,0x00,0x20,0x20,0x70,0x20,0x20,0x20,0x18,0x00,0x00,
            /*--  show:  u  --*/
            0x00,0x00,0x00,0x00,0x00,0xD8,0x48,0x48,0x48,0x3C,0x00,0x00,
            /*--  show:  v  --*/
            0x00,0x00,0x00,0x00,0x00,0xEC,0x48,0x50,0x30,0x20,0x00,0x00,
            /*--  show:  w  --*/
            0x00,0x00,0x00,0x00,0x00,0xA8,0xA8,0x70,0x50,0x50,0x00,0x00,
            /*--  show:  x  --*/
            0x00,0x00,0x00,0x00,0x00,0xD8,0x50,0x20,0x50,0xD8,0x00,0x00,
            /*--  show:  y  --*/
            0x00,0x00,0x00,0x00,0x00,0xEC,0x48,0x50,0x30,0x20,0x20,0xC0,
            /*--  show:  z  --*/
            0x00,0x00,0x00,0x00,0x00,0x78,0x10,0x20,0x20,0x78,0x00,0x00,
            /*--  show:  {  --*/
            0x00,0x18,0x10,0x10,0x10,0x20,0x10,0x10,0x10,0x10,0x18,0x00,
            /*--  show:  |  --*/
            0x10,0x10,0x10,0x10,0x10,0x10,0x10,0x10,0x10,0x10,0x10,0x10,
            /*--  show:  }  --*/
            0x00,0x60,0x20,0x20,0x20,0x10,0x20,0x20,0x20,0x20,0x60,0x00,
            /*--  show:  ~  --*/
            0x40,0xA4,0x18,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00    
            };

            #endregion

            public virtual int Height
            {
                get { return FEZ_Components.FEZTouch.ScreenHeight; }
            }

            public virtual int Width
            {
                get { return FEZ_Components.FEZTouch.ScreenWidth; }
            }
        }
    }
}