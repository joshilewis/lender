﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lending.Execution.DI;
using StructureMap;

namespace Tests
{
    public static class FixtureExtensions
    {
        private static IContainer container;

        public static void SetupContainer(this Fixture fixture, Registry registry)
        {
            container = IoC.Initialize(registry);
        }

        public static IContainer GetContainer(this Fixture fixture)
        {
            return container;
        }

    }
}