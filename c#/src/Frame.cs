﻿using System;

namespace Miqi.Net
{
    public class Frame
    {
        private const byte Fin = 0x80;
        private const byte TwoBytesLengthCode = 126;
        private const byte EightBytesLengthCode = 127;

        public enum Opcodes
        {
            Continuation = 0x0,
            Text = 0x1,
            Binary = 0x2,
            Close = 0x8,
            Ping = 0x9,
            Pong = 0xA
        }

		private bool isFin;
        public bool IsFin { 
			get { return this.isFin; } 
			set { this.isFin = value; }
		}
		
		private bool isMasked;
        public bool IsMasked { get { return this.isMasked; } }
		
		private ulong payloadLength;
        public ulong PayloadLength { 
			get { return this.payloadLength; } 
			set { this.payloadLength = value; }
		}
		
		private int maskingKey;
        public int MaskingKey { 
			get { return this.maskingKey; } 
			set { this.maskingKey = value; }
		}
		
		private byte[] unmaskedPayload;
        public byte[] UnmaskedPayload { 
			get { return this.unmaskedPayload; } 
			set { this.unmaskedPayload = value; }
		}
		
		private Opcodes opcode;
        public Opcodes Opcode { 
			get { return this.opcode; } 
			set { this.opcode = value; }
		}

        public Frame(Opcodes opcode, byte[] payload, bool isFin)
        {
            this.isFin = isFin;
            this.opcode = opcode;
            this.unmaskedPayload = payload;

            if (this.unmaskedPayload != null)
                this.payloadLength = (ulong)this.unmaskedPayload.Length;

            // Server frames are never masked
            this.isMasked = false;
        }

        private Frame()
        {
        }

        public byte[] ToBuffer()
        {
            byte firstByte = (byte)Opcode;
            if (IsFin)
                firstByte |= Fin;

            if (PayloadLength <= 0)
            {
                byte[] buffer = new byte[2];

                buffer[0] = firstByte;
                buffer[1] = (byte)PayloadLength;
                return buffer;
            }
            if (PayloadLength < TwoBytesLengthCode)
            {
                byte[] buffer = new byte[PayloadLength + 2];

                buffer[0] = firstByte;
                buffer[1] = (byte)PayloadLength;
                Array.Copy(UnmaskedPayload, 0, buffer, 2, (int)PayloadLength);
                return buffer;
            }
            if (PayloadLength < (1 << 16))
            {
                byte[] buffer = new byte[PayloadLength + 2 + 2];

                buffer[0] = firstByte;
                buffer[1] = TwoBytesLengthCode;

                byte[] lengthBytes = BitConverter.GetBytes(Convert.ToUInt16(PayloadLength));
                Array.Reverse(lengthBytes);
                Array.Copy(lengthBytes, 0, buffer, 2, 2);

                Array.Copy(UnmaskedPayload, 0, buffer, 4, (int)PayloadLength);
                return buffer;
            }
            else
            {
                byte[] buffer = new byte[PayloadLength + 2 + 8];

                buffer[0] = firstByte;
                buffer[1] = EightBytesLengthCode;

                byte[] lengthBytes = BitConverter.GetBytes(PayloadLength);
                Array.Copy(lengthBytes, 0, buffer, 2, 8);
                Array.Reverse(lengthBytes);

                Array.Copy(UnmaskedPayload, 0, buffer, 10, (int)PayloadLength);
                return buffer;
            }
        }

        public static Frame FromBuffer(byte[] buffer)
        {
            Frame frame = new Frame();

            // If no extended payload length and no mask are used, the payload starts at the 3rd byte
            int payloadStartIndex = 2;

            byte firstNibble = (byte)(buffer[0] & 0xF0);
            byte secondNibble = (byte)(buffer[0] & 0x0F);

            // When the first bit of the first byte is set,
            // It means that the current frame is the final frame of a message
            if (firstNibble == Fin)
                frame.IsFin = true;

            //  The opcode consists of the last four bits in the first byte
            frame.Opcode = (Opcodes)secondNibble;

            // The last bit of the second byte is the masking bit
            bool isMasked = Convert.ToBoolean((buffer[1] & 0x80) >> 7);

            // Payload length is stored in the first seven bits of the second byte
            ulong payloadLength = (ulong)(buffer[1] & 0x7F);

            // From RFC-6455 - Section 5.2
            // "If 126, the following 2 bytes interpreted as a 16-bit unsigned integer are the payload length
            // (expressed in network byte order)"
            if (payloadLength == TwoBytesLengthCode)
            {
                Array.Reverse(buffer, payloadStartIndex, 2);
                payloadLength = BitConverter.ToUInt16(buffer, payloadStartIndex);
                payloadStartIndex += 2;
            }

            // From RFC-6455 - Section 5.2
            // "If 127, the following 8 bytes interpreted as a 64-bit unsigned integer (the most significant bit MUST be 0) 
            // are the payload length (expressed in network byte order)"
            else if (payloadLength == EightBytesLengthCode)
            {
                Array.Reverse(buffer, payloadStartIndex, 8);
                payloadLength = BitConverter.ToUInt64(buffer, payloadStartIndex);
                payloadStartIndex += 8;
            }

            frame.PayloadLength = payloadLength;

            // From RFC-6455 - Section 5.2
            // "All frames sent from the client to the server are masked by a
            // 32-bit value that is contained within the frame.  This field is
            // present if the mask bit is set to 1 and is absent if the mask bit
            // is set to 0."
            if (isMasked)
            {
                frame.MaskingKey = BitConverter.ToInt32(buffer, payloadStartIndex);
                payloadStartIndex += 4;
            }

            byte[] content = new byte[frame.PayloadLength];
            Array.Copy(buffer, payloadStartIndex, content, 0, (int)frame.PayloadLength);

            if (isMasked)
                UnMask(content, frame.MaskingKey);

            frame.UnmaskedPayload = content;
            return frame;
        }

        private static void UnMask (byte[] payload, int maskingKey)
        {
            int currentMaskIndex = 0;

            byte[] byteKeys = BitConverter.GetBytes(maskingKey);
            for (int index = 0; index < payload.Length; ++index)
            {
                payload[index] = (byte)(payload[index] ^ byteKeys[currentMaskIndex]);
                currentMaskIndex = (++currentMaskIndex)%4;
            }
        }

        public static Frame CreateClosingFrame()
        {
            // For simplicy, code and reason are not provided.
            return new Frame(Opcodes.Close, null, true);
        }
    }
}