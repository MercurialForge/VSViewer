using System;
using System.ComponentModel;
using System.Runtime.InteropServices;



namespace SharpDX.Direct3D11
{
    public class ConstantBuffer<T> : IDisposable, INotifyPropertyChanged
        where T : struct
    {
        /// <summary>
        /// 
        /// </summary>
        public Direct3D11.Buffer Buffer { get { return m_buffer; } }

        /// <summary>
        /// 
        /// </summary>
        public Direct3D11.Device Device { get { return m_device; } }

        /// <summary>
        /// 
        /// </summary>
        public ConstantBuffer(Direct3D11.Device device)
            : this(device, new Direct3D11.BufferDescription
            {
                Usage = Direct3D11.ResourceUsage.Default,
                BindFlags = Direct3D11.BindFlags.ConstantBuffer,
                CpuAccessFlags = Direct3D11.CpuAccessFlags.None,
                OptionFlags = Direct3D11.ResourceOptionFlags.None,
                StructureByteStride = 0
            })
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public ConstantBuffer(Direct3D11.Device device, Direct3D11.BufferDescription desc)
        {
            desc.SizeInBytes = Marshal.SizeOf(typeof(T));

            if (device == null)
                throw new ArgumentNullException("device");

            this.m_device = device;
            //_device.AddReference();

            m_buffer = new Direct3D11.Buffer(device, desc);
            m_dataStream = new DataStream(desc.SizeInBytes, true, true);
        }

        /// <summary>
        /// 
        /// </summary>        
        ~ConstantBuffer()
        {
            Dispose(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        /// <summary>
        /// 
        /// </summary>
        private void Dispose(bool disposing)
        {
            if (m_device == null)
                return;

            if (disposing)
                m_dataStream.Dispose();
            // NOTE: SharpDX 1.3 requires explicit Dispose() of all resource
            m_device.Dispose();
            m_buffer.Dispose();
            m_device = null;
            m_buffer = null;
        }

        /// <summary>
        /// 
        /// </summary>
        public T Value
        {
            get { return m_bufvalue; }
            set
            {
                if (m_device == null)
                    throw new ObjectDisposedException(GetType().Name);

                m_bufvalue = value;

                Marshal.StructureToPtr(value, m_dataStream.DataPointer, false);
                var dataBox = new DataBox(m_dataStream.DataPointer, 0, 0);         
                m_device.ImmediateContext.UpdateSubresource(dataBox, m_buffer, 0);

                OnPropertyChanged("Value");
            }
        }
        


        private Direct3D11.Device m_device;
        private Direct3D11.Buffer m_buffer;
        private DataStream m_dataStream;
        private T m_bufvalue;

        #region INotifyPropertyChanged Members

        void OnPropertyChanged(string name)
        {
            var e = PropertyChanged;
            if (e != null)
                e(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}


namespace SharpDX.Direct3D10
{
    public class ConstantBuffer<T> : IDisposable, INotifyPropertyChanged
        where T : struct
    {
        /// <summary>
        /// 
        /// </summary>
        public Direct3D10.Buffer Buffer { get { return m_buffer; } }
       
        /// <summary>
        /// 
        /// </summary>
        public Direct3D10.Device Device { get { return m_device; } }

        /// <summary>
        /// 
        /// </summary>        
        public ConstantBuffer(Direct3D10.Device device)
            : this(device, new Direct3D10.BufferDescription
            {
                Usage = Direct3D10.ResourceUsage.Default,
                BindFlags = Direct3D10.BindFlags.ConstantBuffer,
                CpuAccessFlags = Direct3D10.CpuAccessFlags.None,
                OptionFlags = Direct3D10.ResourceOptionFlags.None,
            })
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <param name="desc"></param>
        public ConstantBuffer(Direct3D10.Device device, Direct3D10.BufferDescription desc)
        {
            desc.SizeInBytes = Marshal.SizeOf(typeof(T));

            if (device == null)
                throw new ArgumentNullException("device");

            this.m_device = device;
            //_device.AddReference();

            m_buffer = new Direct3D10.Buffer(device, desc);
            m_dataStream = new DataStream(desc.SizeInBytes, true, true);
        }

        /// <summary>
        /// 
        /// </summary>
        ~ConstantBuffer()
        {
            Dispose(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        void Dispose(bool disposing)
        {
            if (m_device == null)
                return;

            if (disposing)
                m_dataStream.Dispose();
            // NOTE: SharpDX 1.3 requires explicit Dispose() of all resource
            m_device.Dispose();
            m_buffer.Dispose();
            m_device = null;
            m_buffer = null;
        }

        /// <summary>
        /// 
        /// </summary>
        public T Value
        {
            get { return m_bufvalue; }
            set
            {
                if (m_device == null)
                    throw new ObjectDisposedException(GetType().Name);

                m_bufvalue = value;

                Marshal.StructureToPtr(value, m_dataStream.DataPointer, false);
                var dataBox = new DataBox(m_dataStream.DataPointer, 0, 0);
                m_device.UpdateSubresource(dataBox, m_buffer, 0);

                OnPropertyChanged("Value");
            }
        }
        
        private Direct3D10.Device m_device;
        private Direct3D10.Buffer m_buffer;
        private DataStream m_dataStream;
        private T m_bufvalue;

        #region INotifyPropertyChanged Members

        void OnPropertyChanged(string name)
        {
            var e = PropertyChanged;
            if (e != null)
                e(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

}

