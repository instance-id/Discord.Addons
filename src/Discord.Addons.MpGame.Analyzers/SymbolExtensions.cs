﻿using System;
using Microsoft.CodeAnalysis;

namespace Discord.Addons.MpGame.Analyzers
{
    internal static class SymbolExtensions
    {
        public static bool IsOrDerivesFromType(this ITypeSymbol symbol, Type type)
        {
            if (symbol.MetadataName == type.Name)
                return true;

            if (type.IsInterface)
            {
                foreach (var iface in symbol.AllInterfaces)
                {
                    if (iface.MetadataName == type.Name)
                        return true;
                }
            }
            else
            {
                for (var bType = symbol.BaseType; bType != null; bType = bType.BaseType)
                {
                    if (bType.MetadataName == type.Name)
                        return true;
                }
            }
            return false;
        }
    }
}