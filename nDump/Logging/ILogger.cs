﻿using System;

namespace nDump.Logging
{
    public interface ILogger
    {
        void Log(String message);
        void Log(Exception ex);
    }
}