// Add these lines to CreateContentInfo in Splitter sample
#if false
            int i;
            IMFMediaBuffer pb;
            IMFPresentationDescriptor pd;
            IPropertyStore ps;

            ppContentInfo.GenerateHeader(null, out i);
            MFExtern.MFCreateMemoryBuffer(i, out pb);
            ppContentInfo.GenerateHeader(pb, out i);
            ppContentInfo.GeneratePresentationDescriptor(out pd);
            ppContentInfo.GetEncodingConfigurationPropertyStore(1, out ps);
#endif
