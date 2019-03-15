﻿using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.UnitTests.Support;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectManipulation
{
    [TestClass]
    public class SetObjectPropertyTests : BaseTest
    {
        #region Type Parsing

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectProperty_Enum_With_Int()
        {
            AssertEx.Throws<ArgumentException>(
                () => SetObjectProperty(ObjectProperty.IntervalErrorMode, 1),
                "'1' is not a valid value for enum IntervalErrorMode. Please specify one of 'DownImmediately'"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectProperty_Enum_With_Enum()
        {
            SetObjectProperty(ObjectProperty.IntervalErrorMode, IntervalErrorMode.DownImmediately, ((int)IntervalErrorMode.DownImmediately).ToString());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectProperty_Bool_With_Bool()
        {
            SetObjectProperty(ObjectProperty.InheritInterval, false, "0");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectProperty_Bool_With_Int()
        {
            SetObjectProperty(ObjectProperty.InheritInterval, true, "1");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectProperty_Int_With_Enum()
        {
            AssertEx.Throws<InvalidTypeException>(
                () => SetObjectProperty(ObjectProperty.DBPort, Status.Up, "8"),
                "Expected type: 'System.Int32'. Actual type: 'PrtgAPI.Status'"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectProperty_Int_With_Int()
        {
            SetObjectProperty(ObjectProperty.DBPort, 8080);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectProperty_Double_With_Int()
        {
            SetObjectProperty(ChannelProperty.ScalingDivision, 10);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectProperty_Int_With_Double()
        {
            double val = 8080.0;
            SetObjectProperty(ObjectProperty.DBPort, val, "8080");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectProperty_Int_With_Bool()
        {
            AssertEx.Throws<InvalidTypeException>(
                () => SetObjectProperty(ObjectProperty.DBPort, true, "1"),
                "Expected type: 'System.Int32'. Actual type: 'System.Boolean'"
            );
        }

        #endregion
        #region Normal

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectProperty_ReverseDependencyProperty()
        {
            SetObjectProperty(ChannelProperty.ColorMode, AutoMode.Manual, "1");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SetObjectProperty_CanExecuteAsync()
        {
            var client = Initialize_Client(new SetObjectPropertyResponse<ObjectProperty>(ObjectProperty.DBPort, "8080"));

            await client.SetObjectPropertyAsync(1001, ObjectProperty.DBPort, 8080);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectProperty_CanSetLocation()
        {
            SetObjectProperty(ObjectProperty.Location, "23 Fleet Street, Boston", "23 Fleet St, Boston, MA 02113, USA");
        }

        #region Google Location

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Location_Google_Deserializes()
        {
            var client = GetLocationClient(RequestVersion.v14_4);

            var result = client.ResolveAddress("Google", CancellationToken.None);

            Assert.AreEqual("23 Fleet St, Boston, MA 02113, USA", result.Address);
            Assert.AreEqual(42.3643847, result.Latitude);
            Assert.AreEqual(-71.0527997, result.Longitude);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Location_Google_DeserializesAsync()
        {
            var client = GetLocationClient(RequestVersion.v14_4);

            var result = await client.ResolveAddressAsync("Google", CancellationToken.None);

            Assert.AreEqual("23 Fleet St, Boston, MA 02113, USA", result.Address);
            Assert.AreEqual(42.3643847, result.Latitude);
            Assert.AreEqual(-71.0527997, result.Longitude);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Location_Google_CorrectUrl()
        {
            Execute(
                c => c.ResolveAddress("google google", CancellationToken.None),
                "geolocator.htm?cache=false&dom=0&path=google%2Bgoogle&username",
                version: RequestVersion.v14_4
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Location_Google_ResolvesNothing()
        {
            var client = GetLocationClient(RequestVersion.v14_4);

            var location = client.ResolveAddress(null, CancellationToken.None);

            Assert.AreEqual(null, location.ToString());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Location_Google_ResolvesNothingAsync()
        {
            var client = GetLocationClient(RequestVersion.v14_4);

            var location = await client.ResolveAddressAsync(null, CancellationToken.None);

            Assert.AreEqual(null, location.ToString());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Location_Google_FailsToResolve()
        {
            var client = Initialize_Client(new GeoLocatorResponse(GeoLocatorResponseType.NoResults), RequestVersion.v14_4);

            AssertEx.Throws<PrtgRequestException>(() => client.ResolveAddress("something", CancellationToken.None), "Could not resolve 'something' to an actual address");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Location_Google_FailsToResolveAsync()
        {
            var client = Initialize_Client(new GeoLocatorResponse(GeoLocatorResponseType.NoResults), RequestVersion.v14_4);

            await AssertEx.ThrowsAsync<PrtgRequestException>(async () => await client.ResolveAddressAsync("something", CancellationToken.None), "Could not resolve 'something' to an actual address");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Location_Google_ProviderUnavailable()
        {
            var client = Initialize_Client(new GeoLocatorResponse(GeoLocatorResponseType.NoProvider), RequestVersion.v14_4);

            AssertEx.Throws<PrtgRequestException>(() => client.ResolveAddress("something", CancellationToken.None), "the PRTG map provider is not currently available");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Location_Google_NoAPIKey()
        {
            var client = Initialize_Client(new GeoLocatorResponse(GeoLocatorResponseType.NoAPIKey), RequestVersion.v14_4);

            AssertEx.Throws<PrtgRequestException>(() => client.ResolveAddress("google", CancellationToken.None), "Could not resolve 'google' to an actual address: server responded with 'Keyless access to Google Maps Platform is deprecated. Please use an API key with all your API calls to avoid service interruption. For further details please refer to http://g.co/dev/maps-no-account. OVER_QUERY_LIMIT'");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Location_Google_NoAPIKeyAsync()
        {
            var client = Initialize_Client(new GeoLocatorResponse(GeoLocatorResponseType.NoAPIKey), RequestVersion.v14_4);

            await AssertEx.ThrowsAsync<PrtgRequestException>(async () => await client.ResolveAddressAsync("google", CancellationToken.None), "Could not resolve 'google' to an actual address: server responded with 'Keyless access to Google Maps Platform is deprecated. Please use an API key with all your API calls to avoid service interruption. For further details please refer to http://g.co/dev/maps-no-account. OVER_QUERY_LIMIT'");
        }

        #endregion
        #region Here Location

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Location_Here_Deserializes()
        {
            var client = GetLocationClient(RequestVersion.v18_1);

            var result = client.ResolveAddress("HERE", CancellationToken.None);

            Assert.AreEqual("100 HERE Lane", result.Address);
            Assert.AreEqual(62.3643847, result.Latitude);
            Assert.AreEqual(-91.0527997, result.Longitude);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Location_Here_DeserializesAsync()
        {
            var client = GetLocationClient(RequestVersion.v18_1);

            var result = await client.ResolveAddressAsync("HERE", CancellationToken.None);

            Assert.AreEqual("100 HERE Lane", result.Address);
            Assert.AreEqual(62.3643847, result.Latitude);
            Assert.AreEqual(-91.0527997, result.Longitude);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Location_Here_CorrectUrl()
        {
            Execute(
                c => c.ResolveAddress("here here", CancellationToken.None),
                "geolocator.htm?cache=false&dom=2&path=here%2Bhere&username",
                version: RequestVersion.v17_4
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Location_Here_ResolvesNothing()
        {
            var client = GetLocationClient(RequestVersion.v17_4);

            var location = client.ResolveAddress(null, CancellationToken.None);

            Assert.AreEqual(null, location.ToString());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Location_Here_ResolvesNothingAsync()
        {
            var client = GetLocationClient(RequestVersion.v17_4);

            var location = await client.ResolveAddressAsync(null, CancellationToken.None);

            Assert.AreEqual(null, location.ToString());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Location_Here_FailsToResolve()
        {
            var client = Initialize_Client(new GeoLocatorResponse(GeoLocatorResponseType.NoResults), RequestVersion.v18_1);

            AssertEx.Throws<PrtgRequestException>(() => client.ResolveAddress("something", CancellationToken.None), "Could not resolve 'something' to an actual address");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Location_Here_FailsToResolveAsync()
        {
            var client = Initialize_Client(new GeoLocatorResponse(GeoLocatorResponseType.NoResults), RequestVersion.v18_1);

            await AssertEx.ThrowsAsync<PrtgRequestException>(async () => await client.ResolveAddressAsync("something", CancellationToken.None), "Could not resolve 'something' to an actual address");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Location_Here_ProviderUnavailable()
        {
            var client = Initialize_Client(new GeoLocatorResponse(GeoLocatorResponseType.NoProvider), RequestVersion.v18_1);

            AssertEx.Throws<PrtgRequestException>(() => client.ResolveAddress("something", CancellationToken.None), "the PRTG map provider is not currently available");
        }

        #endregion
        #region Coordinates Location

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Location_Coordinates_Matches_InputString()
        {
            var lat = "40.71455";
            var lon = "-74.00714";

            var url = $"editsettings?id=1001&location_={lat}%2C+{lon}&lonlat_={lon}%2C{lat}&locationgroup=0&username";

            Execute(c =>
            {
                var location = c.ResolveAddress($"{lat}, {lon}", CancellationToken.None);

                Assert.AreEqual(lat, location.Latitude.ToString(), "Latitude was incorrect");
                Assert.AreEqual(lon, location.Longitude.ToString(), "Longitude was incorrect");
                Assert.AreEqual($"{lat}, {lon}", location.Address, "Address was incorrect");

                c.SetObjectProperty(1001, ObjectProperty.Location, location);
            }, url);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Location_Coordinates_Matches_InputStringAsync()
        {
            var lat = "40.71455";
            var lon = "-74.00714";

            var url = $"editsettings?id=1001&location_={lat}%2C+{lon}&lonlat_={lon}%2C{lat}&locationgroup=0&username";

            await ExecuteAsync(async c =>
            {
                var location = await c.ResolveAddressAsync($"{lat}, {lon}", CancellationToken.None);

                Assert.AreEqual(lat, location.Latitude.ToString(), "Latitude was incorrect");
                Assert.AreEqual(lon, location.Longitude.ToString(), "Longitude was incorrect");
                Assert.AreEqual($"{lat}, {lon}", location.Address, "Address was incorrect");

                await c.SetObjectPropertyAsync(1001, ObjectProperty.Location, location);
            }, url);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Location_Coordinates_Array()
        {
            var lat = "40.71455";
            var lon = "-74.00714";

            Execute(
                c => c.SetObjectProperty(1001, ObjectProperty.Location, new[] {40.71455, -74.00714}),
                $"editsettings?id=1001&location_={lat}%2C+{lon}&lonlat_={lon}%2C{lat}&locationgroup=0&username"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Location_Coordinates_ArrayAsync()
        {
            var lat = "40.71455";
            var lon = "-74.00714";

            await ExecuteAsync(
                async c => await c.SetObjectPropertyAsync(1001, ObjectProperty.Location, new[] { 40.71455, -74.00714 }),
                $"editsettings?id=1001&location_={lat}%2C+{lon}&lonlat_={lon}%2C{lat}&locationgroup=0&username"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Location_Coordinates_Removes_InvalidCharacters()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            Action<string, string, string> validate = (str, expected, message) =>
            {
                var location = client.ResolveAddress(str, CancellationToken.None);

                Assert.AreEqual(expected, location.Address, message);
            };

            var lat = "40.71455";
            var lon = "-74.00714";

            var expectedStr = $"{lat}, {lon}";

            validate($"{lat}\r{lon}",          expectedStr, "\\r");
            validate($"{lat}\n{lon}",          expectedStr, "\\n");
            validate($"{lat}\r\n{lon}",        expectedStr, "\\r\\n");
            validate($"{lat}\r\n\r{lon}",      expectedStr, "\\r\\n\\r");
            validate($"{lat}, {lon} {{blah}}", expectedStr, "{{blah}}");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Location_Coordinates_Truncates_MultiplePeriods()
        {
            var lat = "40.7145.5";
            var lon = "-74.00714";

            var client = Initialize_Client(new MultiTypeResponse());

            var location = client.ResolveAddress($"{lat}, {lon}", CancellationToken.None);

            Assert.AreEqual("40.7145", location.Latitude.ToString(), "Latitude was incorrect");
            Assert.AreEqual(lon, location.Longitude.ToString(), "Longitude was incorrect");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Location_Coordinates_Matches_WithoutComma()
        {
            var lat = "40.71455";
            var lon = "-74.00714";

            Execute(
                c => c.SetObjectProperty(1001, ObjectProperty.Location, $"{lat} {lon}"),
                $"editsettings?id=1001&location_={lat}%2C+{lon}&lonlat_={lon}%2C{lat}&locationgroup=0&username"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Location_Coordinates_Matches_WithoutCommaOrSpace()
        {
            var lat = "40.71455";
            var lon = "-74.00714";

            Execute(
                c => c.SetObjectProperty(1001, ObjectProperty.Location, $"{lat}{lon}"),
                $"editsettings?id=1001&location_={lat}%2C+{lon}&lonlat_={lon}%2C{lat}&locationgroup=0&username"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Location_Coordinates_Ignores_ThreeOrMoreGroups()
        {
            var lat = "40.7145";
            var lon = "-74.00714";
            var other = "101.6262";

            Execute(
                c => c.ResolveAddress($"{lat}, {lon}, {other}", CancellationToken.None),
                new[]
                {
                    "https://prtg.example.com/api/getstatus.htm?id=0&username=username&passhash=12345678",
                    "https://prtg.example.com/api/geolocator.htm?cache=false&dom=0&path=40.7145%2C%2B-74.00714%2C%2B101.6262&username=username&passhash=12345678"
                }
            );
        }

        #endregion
        #region Location Label

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Location_Label_NewLine_Coordinates()
        {
            Execute(
                c => c.SetObjectProperty(1001, ObjectProperty.Location, "Headquarters\n12.3456, -7.8910"),
                "editsettings?id=1001&location_=Headquarters%0A12.3456%2C+-7.891&lonlat_=-7.891%2C12.3456&locationgroup=0&username"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Location_Label_NewLine_CoordinatesAsync()
        {
            await ExecuteAsync(
                async c => await c.SetObjectPropertyAsync(1001, ObjectProperty.Location, "Headquarters\n12.3456, -7.8910"),
                "editsettings?id=1001&location_=Headquarters%0A12.3456%2C+-7.891&lonlat_=-7.891%2C12.3456&locationgroup=0&username"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Location_Label_CarriageReturn_Coordinates()
        {
            Execute(
                c => c.SetObjectProperty(1001, ObjectProperty.Location, "Headquarters\r12.3456, -7.8910"),
                "editsettings?id=1001&location_=12.3456%2C+-7.891&lonlat_=-7.891%2C12.3456&locationgroup=0&username"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Location_Label_CarriageReturn_CoordinatesAsync()
        {
            await ExecuteAsync(
                async c => await c.SetObjectPropertyAsync(1001, ObjectProperty.Location, "Headquarters\r12.3456, -7.8910"),
                "editsettings?id=1001&location_=12.3456%2C+-7.891&lonlat_=-7.891%2C12.3456&locationgroup=0&username"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Location_Label_NewLineCarriageReturn_Coordinates()
        {
            Execute(
                c => c.SetObjectProperty(1001, ObjectProperty.Location, "Headquarters\r\n12.3456, -7.8910"),
                "editsettings?id=1001&location_=Headquarters%0A12.3456%2C+-7.891&lonlat_=-7.891%2C12.3456&locationgroup=0&username"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Location_Label_NewLineCarriageReturn_CoordinatesAsync()
        {
            await ExecuteAsync(
                async c => await c.SetObjectPropertyAsync(1001, ObjectProperty.Location, "Headquarters\r\n12.3456, -7.8910"),
                "editsettings?id=1001&location_=Headquarters%0A12.3456%2C+-7.891&lonlat_=-7.891%2C12.3456&locationgroup=0&username"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Location_Label_NewLine_Address()
        {
            Execute(
                c => c.SetObjectProperty(1001, ObjectProperty.Location, "Headquarters\n23 Fleet Street"),
                new[]
                {
                    "https://prtg.example.com/api/getstatus.htm?id=0&username=username&passhash=12345678",
                    "https://prtg.example.com/api/geolocator.htm?cache=false&dom=0&path=23%2BFleet%2BStreet&username=username&passhash=12345678",
                    "https://prtg.example.com/editsettings?id=1001&location_=Headquarters%0A23+Fleet+St%2C+Boston%2C+MA+02113%2C+USA&lonlat_=-71.0527997%2C42.3643847&locationgroup=0&username=username&passhash=12345678"
                }
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Location_Label_NewLine_AddressAsync()
        {
            await ExecuteAsync(
                async c => await c.SetObjectPropertyAsync(1001, ObjectProperty.Location, "Headquarters\n23 Fleet Street"),
                new[]
                {
                    "https://prtg.example.com/api/getstatus.htm?id=0&username=username&passhash=12345678",
                    "https://prtg.example.com/api/geolocator.htm?cache=false&dom=0&path=23%2BFleet%2BStreet&username=username&passhash=12345678",
                    "https://prtg.example.com/editsettings?id=1001&location_=Headquarters%0A23+Fleet+St%2C+Boston%2C+MA+02113%2C+USA&lonlat_=-71.0527997%2C42.3643847&locationgroup=0&username=username&passhash=12345678"
                }
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Location_Label_CarriageReturn_Address()
        {
            Execute(
                c => c.SetObjectProperty(1001, ObjectProperty.Location, "Headquarters\r23 Fleet Street"),
                new[]
                {
                    "https://prtg.example.com/api/getstatus.htm?id=0&username=username&passhash=12345678",
                    "https://prtg.example.com/api/geolocator.htm?cache=false&dom=0&path=Headquarters%0D23%2BFleet%2BStreet&username=username&passhash=12345678",
                    "https://prtg.example.com/editsettings?id=1001&location_=23+Fleet+St%2C+Boston%2C+MA+02113%2C+USA&lonlat_=-71.0527997%2C42.3643847&locationgroup=0&username=username&passhash=12345678"
                }
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Location_Label_CarriageReturn_AddressAsync()
        {
            await ExecuteAsync(
                async c => await c.SetObjectPropertyAsync(1001, ObjectProperty.Location, "Headquarters\r23 Fleet Street"),
                new[]
                {
                    "https://prtg.example.com/api/getstatus.htm?id=0&username=username&passhash=12345678",
                    "https://prtg.example.com/api/geolocator.htm?cache=false&dom=0&path=Headquarters%0D23%2BFleet%2BStreet&username=username&passhash=12345678",
                    "https://prtg.example.com/editsettings?id=1001&location_=23+Fleet+St%2C+Boston%2C+MA+02113%2C+USA&lonlat_=-71.0527997%2C42.3643847&locationgroup=0&username=username&passhash=12345678"
                }
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Location_Label_NewLineCarriageReturn_Address()
        {
            Execute(
                c => c.SetObjectProperty(1001, ObjectProperty.Location, "Headquarters\r\n23 Fleet Street"),
                new[]
                {
                    "https://prtg.example.com/api/getstatus.htm?id=0&username=username&passhash=12345678",
                    "https://prtg.example.com/api/geolocator.htm?cache=false&dom=0&path=23%2BFleet%2BStreet&username=username&passhash=12345678",
                    "https://prtg.example.com/editsettings?id=1001&location_=Headquarters%0A23+Fleet+St%2C+Boston%2C+MA+02113%2C+USA&lonlat_=-71.0527997%2C42.3643847&locationgroup=0&username=username&passhash=12345678"
                }
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Location_Label_NewLineCarriageReturn_AddressAsync()
        {
            await ExecuteAsync(
                async c => await c.SetObjectPropertyAsync(1001, ObjectProperty.Location, "Headquarters\n23 Fleet Street"),
                new[]
                {
                    "https://prtg.example.com/api/getstatus.htm?id=0&username=username&passhash=12345678",
                    "https://prtg.example.com/api/geolocator.htm?cache=false&dom=0&path=23%2BFleet%2BStreet&username=username&passhash=12345678",
                    "https://prtg.example.com/editsettings?id=1001&location_=Headquarters%0A23+Fleet+St%2C+Boston%2C+MA+02113%2C+USA&lonlat_=-71.0527997%2C42.3643847&locationgroup=0&username=username&passhash=12345678"
                }
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Location_Label_Parameter_Coordinates()
        {
            Execute(
                c => c.SetObjectProperty(
                    1001,
                    new PropertyParameter(ObjectProperty.Location, "12.3456, -7.8910"),
                    new PropertyParameter(ObjectProperty.LocationName, "Headquarters")
                ),
                "editsettings?id=1001&location_=Headquarters%0A12.3456%2C+-7.891&lonlat_=-7.891%2C12.3456&locationgroup=0&username"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Location_Label_Parameter_CoordinatesAsync()
        {
            await ExecuteAsync(
                async c => await c.SetObjectPropertyAsync(
                    1001,
                    new PropertyParameter(ObjectProperty.Location, "12.3456, -7.8910"),
                    new PropertyParameter(ObjectProperty.LocationName, "Headquarters")
                ),
                "editsettings?id=1001&location_=Headquarters%0A12.3456%2C+-7.891&lonlat_=-7.891%2C12.3456&locationgroup=0&username"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Location_Label_Parameter_Address()
        {
            Execute(
                c => c.SetObjectProperty(
                    1001,
                    new PropertyParameter(ObjectProperty.Location, "23 Fleet Street"),
                    new PropertyParameter(ObjectProperty.LocationName, "Headquarters")
                ),
                new[]
                {
                    "https://prtg.example.com/api/getstatus.htm?id=0&username=username&passhash=12345678",
                    "https://prtg.example.com/api/geolocator.htm?cache=false&dom=0&path=23%2BFleet%2BStreet&username=username&passhash=12345678",
                    "https://prtg.example.com/editsettings?id=1001&location_=Headquarters%0A23+Fleet+St%2C+Boston%2C+MA+02113%2C+USA&lonlat_=-71.0527997%2C42.3643847&locationgroup=0&username=username&passhash=12345678"
                }
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Location_Label_Parameter_AddressAsync()
        {
            await ExecuteAsync(
                async c => await c.SetObjectPropertyAsync(
                    1001,
                    new PropertyParameter(ObjectProperty.Location, "23 Fleet Street"),
                    new PropertyParameter(ObjectProperty.LocationName, "Headquarters")
                ),
                new[]
                {
                    "https://prtg.example.com/api/getstatus.htm?id=0&username=username&passhash=12345678",
                    "https://prtg.example.com/api/geolocator.htm?cache=false&dom=0&path=23%2BFleet%2BStreet&username=username&passhash=12345678",
                    "https://prtg.example.com/editsettings?id=1001&location_=Headquarters%0A23+Fleet+St%2C+Boston%2C+MA+02113%2C+USA&lonlat_=-71.0527997%2C42.3643847&locationgroup=0&username=username&passhash=12345678"
                }
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Location_Label_WithoutLocation()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            AssertEx.Throws<InvalidOperationException>(
                () => client.SetObjectProperty(1001, ObjectProperty.LocationName, "Test"),
                "ObjectProperty 'LocationName' must be used in conjunction with property 'Location'"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Location_Label_WithoutLocationAsync()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            await AssertEx.ThrowsAsync<InvalidOperationException>(
                async () => await client.SetObjectPropertyAsync(1001, ObjectProperty.LocationName, "Test"),
                "ObjectProperty 'LocationName' must be used in conjunction with property 'Location'"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Location_Label_Coordinates_EndsWithNewline()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            Execute(
                c => c.SetObjectProperty(1001, ObjectProperty.Location, "1.1, 2.2\n"),
                "editsettings?id=1001&location_=1.1%2C+2.2&lonlat_=2.2%2C1.1&locationgroup=0&username"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Location_Label_Coordinates_EndsWithNewlineAsync()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            await ExecuteAsync(
                async c => await c.SetObjectPropertyAsync(1001, ObjectProperty.Location, "1.1, 2.2\n"),
                "editsettings?id=1001&location_=1.1%2C+2.2&lonlat_=2.2%2C1.1&locationgroup=0&username"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Location_Label_Address_EndsWithNewline()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            Execute(
                c => c.SetObjectProperty(1001, ObjectProperty.Location, "23 Fleet Street\n"),
                new[]
                {
                    "https://prtg.example.com/api/getstatus.htm?id=0&username=username&passhash=12345678",
                    "https://prtg.example.com/api/geolocator.htm?cache=false&dom=0&path=23%2BFleet%2BStreet%0A&username=username&passhash=12345678",
                    "https://prtg.example.com/editsettings?id=1001&location_=23+Fleet+St%2C+Boston%2C+MA+02113%2C+USA&lonlat_=-71.0527997%2C42.3643847&locationgroup=0&username=username&passhash=12345678"
                }
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Location_Label_Address_EndsWithNewlineAsync()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            await ExecuteAsync(
                async c => await c.SetObjectPropertyAsync(1001, ObjectProperty.Location, "23 Fleet Street\n"),
                new[]
                {
                    "https://prtg.example.com/api/getstatus.htm?id=0&username=username&passhash=12345678",
                    "https://prtg.example.com/api/geolocator.htm?cache=false&dom=0&path=23%2BFleet%2BStreet%0A&username=username&passhash=12345678",
                    "https://prtg.example.com/editsettings?id=1001&location_=23+Fleet+St%2C+Boston%2C+MA+02113%2C+USA&lonlat_=-71.0527997%2C42.3643847&locationgroup=0&username=username&passhash=12345678"
                }
            );
        }

        #endregion

        private PrtgClient GetLocationClient(RequestVersion version)
        {
            var client = Initialize_Client(
                new SetObjectPropertyResponse<ObjectProperty>(ObjectProperty.Location, null),
                version
            );

            return client;
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectProperty_CanExecute() =>
            Execute(c => c.SetObjectPropertyRaw(1001, "name_", "testName"), "editsettings?id=1001&name_=testName");

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SetObjectPropertyRaw_CanExecuteAsync() =>
            await ExecuteAsync(async c => await c.SetObjectPropertyRawAsync(1001, "name_", "testName"), "editsettings?id=1001&name_=testName");

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SetChannelProperty_CanExecuteAsync()
        {
            var client = Initialize_Client(new SetObjectPropertyResponse<ChannelProperty>(ChannelProperty.LimitsEnabled, "1"));

            await client.SetObjectPropertyAsync(1001, 1, ChannelProperty.LimitsEnabled, true);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectProperty_CanNullifyValue()
        {
            SetObjectProperty(ChannelProperty.UpperErrorLimit, null, string.Empty);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SetObjectProperty_CanSetLocationAsync()
        {
            var client = Initialize_Client(new SetObjectPropertyResponse<ObjectProperty>(ObjectProperty.Location, "23 Fleet St, Boston, MA 02113, USA"));

            await client.SetObjectPropertyAsync(1, ObjectProperty.Location, "23 Fleet Street");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectProperty_Array()
        {
            var channels = new[]
            {
                "#1: First Channel",
                "channel(1001,1)",
                "#2: Second Channel",
                "channel(2001,1)"
            };

            SetObjectProperty(ObjectProperty.ChannelDefinition, channels, string.Join("\n", channels));
        }

        private void SetObjectProperty(ObjectProperty property, object value, string expectedSerializedValue = null)
        {
            if (expectedSerializedValue == null)
                expectedSerializedValue = value.ToString();

            var response = new SetObjectPropertyResponse<ObjectProperty>(property, expectedSerializedValue);

            var client = Initialize_Client(response);

            client.SetObjectProperty(1, property, value);
        }

        private void SetObjectProperty(ChannelProperty property, object value, string expectedSerializedValue = null)
        {
            if (expectedSerializedValue == null)
                expectedSerializedValue = value.ToString();

            var client = Initialize_Client(new SetObjectPropertyResponse<ChannelProperty>(property, expectedSerializedValue));
            client.SetObjectProperty(1, 1, property, value);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectProperty_DecimalPoint_AmericanCulture()
        {
            TestCustomCulture(() => SetObjectProperty(ChannelProperty.ScalingDivision, "1.1", "1.1"), new CultureInfo("en-US"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectProperty_DecimalPoint_EuropeanCulture()
        {
            TestCustomCulture(() =>
            {
                SetObjectProperty(ChannelProperty.ScalingDivision, "1,1", "1,1");
                SetObjectProperty(ChannelProperty.ScalingDivision, 1.1, "1,1");
            }, new CultureInfo("de-DE"));
        }

        private void TestCustomCulture(Action action, CultureInfo newCulture)
        {
            var originalCulture = Thread.CurrentThread.CurrentCulture;

            try
            {
                Thread.CurrentThread.CurrentCulture = newCulture;

                action();
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }

        #endregion
        #region Multiple

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectProperty_Modifies_MultipleProperties()
        {
            Execute(
                c => c.SetObjectProperty(
                    1001,
                    new PropertyParameter(ObjectProperty.WindowsUserName, "username"),
                    new PropertyParameter(ObjectProperty.WindowsPassword, "password")
                ),
                "id=1001&windowsloginusername_=username&windowsloginpassword_=password&windowsconnection=0"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SetObjectProperty_Modifies_MultiplePropertiesAsync()
        {
            await ExecuteAsync(
                async c => await c.SetObjectPropertyAsync(
                    1001,
                    new PropertyParameter(ObjectProperty.WindowsUserName, "username"),
                    new PropertyParameter(ObjectProperty.WindowsPassword, "password")
                ),
                "id=1001&windowsloginusername_=username&windowsloginpassword_=password&windowsconnection=0"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetChannelProperty_Modifies_MultipleProperties()
        {
            var urls = new[]
            {
                TestHelpers.RequestChannel(1001),
                TestHelpers.RequestChannelProperties(1001, 1),
                TestHelpers.SetChannelProperty("id=1001&limitmaxerror_1=100&limitminerror_1=20&limitmode_1=1&limitmaxerror_1_factor=1&limitminerror_1_factor=1")
            };

            Execute(c => c.SetObjectProperty(
                1001,
                1,
                new ChannelParameter(ChannelProperty.UpperErrorLimit, 100),
                new ChannelParameter(ChannelProperty.LowerErrorLimit, 20)
            ), urls);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SetChannelProperty_Modifies_MultiplePropertiesAsync()
        {
            var urls = new[]
            {
                TestHelpers.RequestChannel(1001),
                TestHelpers.RequestChannelProperties(1001, 1),
                TestHelpers.SetChannelProperty("id=1001&limitmaxerror_1=100&limitminerror_1=20&limitmode_1=1&limitmaxerror_1_factor=1&limitminerror_1_factor=1")
            };

            await ExecuteAsync(
                async c => await c.SetObjectPropertyAsync(
                    1001,
                    1,
                    new ChannelParameter(ChannelProperty.UpperErrorLimit, 100),
                    new ChannelParameter(ChannelProperty.LowerErrorLimit, 20)
                ),
                urls
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetObjectProperty_Modifies_MultipleRawProperties()
        {
            Execute(
                c => c.SetObjectPropertyRaw(
                    1001,
                    new CustomParameter("windowsloginusername_", "username"),
                    new CustomParameter("windowsloginpassword_", "password"),
                    new CustomParameter("windowsconnection", 0)
                ),
                "id=1001&windowsloginusername_=username&windowsloginpassword_=password&windowsconnection=0"
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SetObjectProperty_Modifies_MultipleRawPropertiesAsync()
        {
            await ExecuteAsync(
                async c => await c.SetObjectPropertyRawAsync(
                    1001,
                    new CustomParameter("windowsloginusername_", "username"),
                    new CustomParameter("windowsloginpassword_", "password"),
                    new CustomParameter("windowsconnection", 0)
                ),
                "id=1001&windowsloginusername_=username&windowsloginpassword_=password&windowsconnection=0"
            );
        }

        #endregion
        #region Version Specific
            #region Single

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetChannelProperty_SingleValue_OnlyUpperErrorLimit()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {1, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1" });
            SetChannelProperty(config, RequestVersion.v18_1, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1&limitmaxerror_2=1" });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetChannelProperty_SingleValue_OnlyLowerErrorLimit()
        {
            var matrix = new[]
            {
                new int?[] {null, 1, null, null},
                new int?[] {null, 1, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1" });
            SetChannelProperty(config, RequestVersion.v18_1, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1&limitminerror_2=1" });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetChannelProperty_SingleValue_OnlyUpperWarningLimit()
        {
            var matrix = new[]
            {
                new int?[] {null, null, 1, null},
                new int?[] {null, null, 1, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1" });
            SetChannelProperty(config, RequestVersion.v18_1, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1&limitmaxwarning_2=1" });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetChannelProperty_SingleValue_OnlyLowerWarningLimit()
        {
            var matrix = new[]
            {
                new int?[] {null, null, null, 1},
                new int?[] {null, null, null, 1}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1" });
            SetChannelProperty(config, RequestVersion.v18_1, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1&limitminwarning_2=1" });
        }

            #endregion
            #region Multiple

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetChannelProperty_MultipleValues_OnlyUpperErrorLimit()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {2, null, null, null},
                new int?[] {2, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4, new[] { "id=1001,2001,3001&limiterrormsg_2=test&limitmode_2=1" });
            SetChannelProperty(config, RequestVersion.v18_1, new[]
            {
                "id=2001,3001&limiterrormsg_2=test&limitmode_2=1&limitmaxerror_2=2",
                "id=1001&limiterrormsg_2=test&limitmode_2=1&limitmaxerror_2=1"
            });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetChannelProperty_MultipleValues_OnlyLowerErrorLimit()
        {
            var matrix = new[]
            {
                new int?[] {null, 1, null, null},
                new int?[] {null, 2, null, null},
                new int?[] {null, 2, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            //todo: does this actually group together the sensor IDs to execute against for the request?

            SetChannelProperty(config, RequestVersion.v14_4, new[] { "id=1001,2001,3001&limiterrormsg_2=test&limitmode_2=1" });
            SetChannelProperty(config, RequestVersion.v18_1, new[]
            {
                "id=2001,3001&limiterrormsg_2=test&limitmode_2=1&limitminerror_2=2",
                "id=1001&limiterrormsg_2=test&limitmode_2=1&limitminerror_2=1"
            });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetChannelProperty_MultipleValues_OnlyUpperWarningLimit()
        {
            var matrix = new[]
            {
                new int?[] {null, null, 1, null},
                new int?[] {null, null, 2, null},
                new int?[] {null, null, 2, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4, new[] { "id=1001,2001,3001&limiterrormsg_2=test&limitmode_2=1" });
            SetChannelProperty(config, RequestVersion.v18_1, new[]
            {
                "id=2001,3001&limiterrormsg_2=test&limitmode_2=1&limitmaxwarning_2=2",
                "id=1001&limiterrormsg_2=test&limitmode_2=1&limitmaxwarning_2=1"
            });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetChannelProperty_MultipleValues_OnlyLowerWarningLimit()
        {
            var matrix = new[]
            {
                new int?[] {null, null, null, 1},
                new int?[] {null, null, null, 2},
                new int?[] {null, null, null, 2}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4, new[] { "id=1001,2001,3001&limiterrormsg_2=test&limitmode_2=1" });
            SetChannelProperty(config, RequestVersion.v18_1, new[]
            {
                "id=2001,3001&limiterrormsg_2=test&limitmode_2=1&limitminwarning_2=2",
                "id=1001&limiterrormsg_2=test&limitmode_2=1&limitminwarning_2=1"
                
            });
        }

            #endregion
            #region Version Properties

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetChannelProperty_VersionProperty_ErrorLimitMessage()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {1, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1" });
            SetChannelProperty(config, RequestVersion.v18_1, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1&limitmaxerror_2=1" });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetChannelProperty_VersionProperty_WarningLimitMessage()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {1, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.WarningLimitMessage,
                (object)"test",
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4, new[] { "id=1001,2001&limitwarningmsg_2=test&limitmode_2=1" });
            SetChannelProperty(config, RequestVersion.v18_1, new[] { "id=1001,2001&limitwarningmsg_2=test&limitmode_2=1&limitmaxerror_2=1" });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetChannelProperty_VersionProperty_LimitsEnabled_True()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {1, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.LimitsEnabled,
                (object)true,
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4, new[] { "id=1001,2001&limitmode_2=1" });
            SetChannelProperty(config, RequestVersion.v18_1, new[] { "id=1001,2001&limitmode_2=1&limitmaxerror_2=1" });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetChannelProperty_VersionProperty_LimitsEnabled_False()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {1, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.LimitsEnabled,
                (object)false,
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4, new[] { "id=1001,2001&limitmode_2=0&limitmaxerror_2=&limitmaxwarning_2=&limitminerror_2=&limitminwarning_2=&limiterrormsg_2=&limitwarningmsg_2=" });
            SetChannelProperty(config, RequestVersion.v18_1, new[] { "id=1001,2001&limitmode_2=0&limitmaxerror_2=&limitmaxwarning_2=&limitminerror_2=&limitminwarning_2=&limiterrormsg_2=&limitwarningmsg_2=" });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetChannelProperty_VersionProperty_NormalProperty()
        {
            var matrix = new[]
            {
                new int?[] {null, null, null, null},
                new int?[] {null, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.SpikeFilterMax,
                (object)100,
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4, new[] { "id=1001,2001&spikemax_2=100&spikemode_2=1" });
            SetChannelProperty(config, RequestVersion.v18_1, new[] { "id=1001,2001&spikemax_2=100&spikemode_2=1" });
        }

            #endregion

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetChannelProperty_ThreeValues()
        {
            var matrix = new[]
            {
                new int?[] {1,    null, 2,    1}, //1001 - 2
                new int?[] {1,    null, null, 4}, //2001 - 2
                new int?[] {5,    null, 8,    7}, //3001 - 4
                new int?[] {2,    3,    15,   6}, //4001 - 1
                new int?[] {null, 3,    20,   5}, //5001 - 1
                new int?[] {4,    3,    null, 8}  //6001 - 1
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4, new[] { "id=1001,2001,3001,4001,5001,6001&limiterrormsg_2=test&limitmode_2=1" });
            SetChannelProperty(config, RequestVersion.v18_1, new[]
            {
                "id=4001,5001,6001&limiterrormsg_2=test&limitmode_2=1&limitminerror_2=3",
                "id=1001,2001&limiterrormsg_2=test&limitmode_2=1&limitmaxerror_2=1",
                "id=3001&limiterrormsg_2=test&limitmode_2=1&limitmaxerror_2=5"
            });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetChannelProperty_SomeNull()
        {
            var matrix = new[]
            {
                new int?[] {null, 2, null, null},
                new int?[] {null, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            Action<RequestVersion> action = version => SetChannelProperty(config, version, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1" });

            action(RequestVersion.v14_4);

            var builder = new StringBuilder();
            builder.Append("Cannot set property 'ErrorLimitMessage' to value 'test' for Channel ID 2: ");
            builder.Append("Sensor ID 2001 does not have a limit value defined on it. ");
            builder.Append("Please set one of 'UpperErrorLimit', 'LowerErrorLimit', 'UpperWarningLimit' or 'LowerWarningLimit' first and then try again");

            AssertEx.Throws<InvalidOperationException>(() => action(RequestVersion.v18_1), builder.ToString());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetChannelProperty_AllNull()
        {
            var matrix = new[]
            {
                new int?[] {null, null, null, null},
                new int?[] {null, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            Action<RequestVersion> action = version => SetChannelProperty(config, version, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1" });

            action(RequestVersion.v14_4);

            var builder = new StringBuilder();
            builder.Append("Cannot set property 'ErrorLimitMessage' to value 'test' for Channel ID 2: ");
            builder.Append("Sensor IDs 1001 and 2001 do not have a limit value defined on them. ");
            builder.Append("Please set one of 'UpperErrorLimit', 'LowerErrorLimit', 'UpperWarningLimit' or 'LowerWarningLimit' first and then try again");

            AssertEx.Throws<InvalidOperationException>(() => action(RequestVersion.v18_1), builder.ToString());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetChannelProperty_VersionSpecific_ResolvesChannels()
        {
            var addresses = new[]
            {
                "api/table.xml?content=channels&columns=objid,name,lastvalue&count=*&id=1001",
                "controls/channeledit.htm?id=1001&channel=1",
                "editsettings?id=1001&limiterrormsg_1=hello&limitmode_1=1&limitmaxerror_1=100"
            };

            var property = ChannelProperty.ErrorLimitMessage;
            var val = "hello";

#pragma warning disable 618
            var response = new AddressValidatorResponse(addresses.Select(a => $"https://prtg.example.com/{a}&username=username&passhash=12345678").ToArray(), true);
#pragma warning restore 618

            var client = Initialize_Client(response, RequestVersion.v18_1);

            client.GetVersionClient(new object[] {property}).SetChannelProperty(new[] { 1001 }, 1, null, new[] { new ChannelParameter(property, val) });

            response.AssertFinished();
        }

        private void SetChannelProperty(Tuple<ChannelProperty, object, int?[][]> config, RequestVersion version, string[] addresses)
        {
            var property = config.Item1;
            var val = config.Item2;
            var matrix = config.Item3;

            var channels = matrix.Select(CreateChannel).ToList();

#pragma warning disable 618
            var response = new AddressValidatorResponse(addresses.Select(a => $"https://prtg.example.com/editsettings?{a}&username=username&passhash=12345678").ToArray(), true);
#pragma warning restore 618

            var client = Initialize_Client(response, version);

            client.GetVersionClient(new object[] { property }).SetChannelProperty(channels.Select(c => c.SensorId).ToArray(), 2, channels, new [] {new ChannelParameter(property, val)});

            response.AssertFinished();
        }

        private Channel CreateChannel(int?[] limits, int i)
        {
            return new Channel
            {
                Id = 1,
                SensorId = 1001 + i*1000,
                UpperErrorLimit = limits[0],
                LowerErrorLimit = limits[1],
                UpperWarningLimit = limits[2],
                LowerWarningLimit = limits[3]
            };
        }

        #endregion
        #region Version Specific Async
            #region Single

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SetChannelProperty_SingleValue_OnlyUpperErrorLimitAsync()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {1, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1" });
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1&limitmaxerror_2=1" });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SetChannelProperty_SingleValue_OnlyLowerErrorLimitAsync()
        {
            var matrix = new[]
            {
                new int?[] {null, 1, null, null},
                new int?[] {null, 1, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1" });
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1&limitminerror_2=1" });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SetChannelProperty_SingleValue_OnlyUpperWarningLimitAsync()
        {
            var matrix = new[]
            {
                new int?[] {null, null, 1, null},
                new int?[] {null, null, 1, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1" });
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1&limitmaxwarning_2=1" });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SetChannelProperty_SingleValue_OnlyLowerWarningLimitAsync()
        {
            var matrix = new[]
            {
                new int?[] {null, null, null, 1},
                new int?[] {null, null, null, 1}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1" });
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1&limitminwarning_2=1" });
        }

            #endregion
            #region Multiple

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SetChannelProperty_MultipleValues_OnlyUpperErrorLimitAsync()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {2, null, null, null},
                new int?[] {2, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4, new[] { "id=1001,2001,3001&limiterrormsg_2=test&limitmode_2=1" });
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[]
            {
                "id=2001,3001&limiterrormsg_2=test&limitmode_2=1&limitmaxerror_2=2",
                "id=1001&limiterrormsg_2=test&limitmode_2=1&limitmaxerror_2=1"
            });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SetChannelProperty_MultipleValues_OnlyLowerErrorLimitAsync()
        {
            var matrix = new[]
            {
                new int?[] {null, 1, null, null},
                new int?[] {null, 2, null, null},
                new int?[] {null, 2, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            //todo: does this actually group together the sensor IDs to execute against for the request?

            await SetChannelPropertyAsync(config, RequestVersion.v14_4, new[] { "id=1001,2001,3001&limiterrormsg_2=test&limitmode_2=1" });
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[]
            {
                "id=2001,3001&limiterrormsg_2=test&limitmode_2=1&limitminerror_2=2",
                "id=1001&limiterrormsg_2=test&limitmode_2=1&limitminerror_2=1"
            });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SetChannelProperty_MultipleValues_OnlyUpperWarningLimitAsync()
        {
            var matrix = new[]
            {
                new int?[] {null, null, 1, null},
                new int?[] {null, null, 2, null},
                new int?[] {null, null, 2, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4, new[] { "id=1001,2001,3001&limiterrormsg_2=test&limitmode_2=1" });
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[]
            {
                "id=2001,3001&limiterrormsg_2=test&limitmode_2=1&limitmaxwarning_2=2",
                "id=1001&limiterrormsg_2=test&limitmode_2=1&limitmaxwarning_2=1"
            });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SetChannelProperty_MultipleValues_OnlyLowerWarningLimitAsync()
        {
            var matrix = new[]
            {
                new int?[] {null, null, null, 1},
                new int?[] {null, null, null, 2},
                new int?[] {null, null, null, 2}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4, new[] { "id=1001,2001,3001&limiterrormsg_2=test&limitmode_2=1" });
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[]
            {
                "id=2001,3001&limiterrormsg_2=test&limitmode_2=1&limitminwarning_2=2",
                "id=1001&limiterrormsg_2=test&limitmode_2=1&limitminwarning_2=1"

            });
        }

            #endregion
            #region Version Properties

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SetChannelProperty_VersionProperty_ErrorLimitMessageAsync()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {1, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1" });
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1&limitmaxerror_2=1" });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SetChannelProperty_VersionProperty_WarningLimitMessageAsync()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {1, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.WarningLimitMessage,
                (object)"test",
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4, new[] { "id=1001,2001&limitwarningmsg_2=test&limitmode_2=1" });
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[] { "id=1001,2001&limitwarningmsg_2=test&limitmode_2=1&limitmaxerror_2=1" });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SetChannelProperty_VersionProperty_LimitsEnabled_TrueAsync()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {1, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.LimitsEnabled,
                (object)true,
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4, new[] { "id=1001,2001&limitmode_2=1" });
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[] { "id=1001,2001&limitmode_2=1&limitmaxerror_2=1" });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SetChannelProperty_VersionProperty_LimitsEnabled_FalseAsync()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {1, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.LimitsEnabled,
                (object)false,
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4, new[] { "id=1001,2001&limitmode_2=0&limitmaxerror_2=&limitmaxwarning_2=&limitminerror_2=&limitminwarning_2=&limiterrormsg_2=&limitwarningmsg_2=" });
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[] { "id=1001,2001&limitmode_2=0&limitmaxerror_2=&limitmaxwarning_2=&limitminerror_2=&limitminwarning_2=&limiterrormsg_2=&limitwarningmsg_2=" });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SetChannelProperty_VersionProperty_NormalPropertyAsync()
        {
            var matrix = new[]
            {
                new int?[] {null, null, null, null},
                new int?[] {null, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.SpikeFilterMax,
                (object)100,
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4, new[] { "id=1001,2001&spikemax_2=100&spikemode_2=1" });
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[] { "id=1001,2001&spikemax_2=100&spikemode_2=1" });
        }

            #endregion

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SetChannelProperty_ThreeValuesAsync()
        {
            var matrix = new[]
            {
                new int?[] {1,    null, 2,    1}, //1001 - 2
                new int?[] {1,    null, null, 4}, //2001 - 2
                new int?[] {5,    null, 8,    7}, //3001 - 4
                new int?[] {2,    3,    15,   6}, //4001 - 1
                new int?[] {null, 3,    20,   5}, //5001 - 1
                new int?[] {4,    3,    null, 8}  //6001 - 1
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4, new[] { "id=1001,2001,3001,4001,5001,6001&limiterrormsg_2=test&limitmode_2=1" });
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[]
            {
                "id=4001,5001,6001&limiterrormsg_2=test&limitmode_2=1&limitminerror_2=3",
                "id=1001,2001&limiterrormsg_2=test&limitmode_2=1&limitmaxerror_2=1",
                "id=3001&limiterrormsg_2=test&limitmode_2=1&limitmaxerror_2=5"
            });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SetChannelProperty_SomeNullAsync()
        {
            var matrix = new[]
            {
                new int?[] {null, 2, null, null},
                new int?[] {null, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            Func<RequestVersion, Task> action = async version => await SetChannelPropertyAsync(config, version, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1" });

            await action(RequestVersion.v14_4);

            var builder = new StringBuilder();
            builder.Append("Cannot set property 'ErrorLimitMessage' to value 'test' for Channel ID 2: ");
            builder.Append("Sensor ID 2001 does not have a limit value defined on it. ");
            builder.Append("Please set one of 'UpperErrorLimit', 'LowerErrorLimit', 'UpperWarningLimit' or 'LowerWarningLimit' first and then try again");

            await AssertEx.ThrowsAsync<InvalidOperationException>(async () => await action(RequestVersion.v18_1), builder.ToString());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SetChannelProperty_AllNullAsync()
        {
            var matrix = new[]
            {
                new int?[] {null, null, null, null},
                new int?[] {null, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            Func<RequestVersion, Task> action = async version => await SetChannelPropertyAsync(config, version, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1" });

            await action(RequestVersion.v14_4);

            var builder = new StringBuilder();
            builder.Append("Cannot set property 'ErrorLimitMessage' to value 'test' for Channel ID 2: ");
            builder.Append("Sensor IDs 1001 and 2001 do not have a limit value defined on them. ");
            builder.Append("Please set one of 'UpperErrorLimit', 'LowerErrorLimit', 'UpperWarningLimit' or 'LowerWarningLimit' first and then try again");

            await AssertEx.ThrowsAsync<InvalidOperationException>(async () => await action(RequestVersion.v18_1), builder.ToString());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SetChannelProperty_VersionSpecific_ResolvesChannelsAsync()
        {
            var addresses = new[]
            {
                "api/table.xml?content=channels&columns=objid,name,lastvalue&count=*&id=1001",
                "controls/channeledit.htm?id=1001&channel=1",
                "editsettings?id=1001&limiterrormsg_1=hello&limitmode_1=1&limitmaxerror_1=100"
            };

            var property = ChannelProperty.ErrorLimitMessage;
            var val = "hello";

#pragma warning disable 618
            var response = new AddressValidatorResponse(addresses.Select(a => $"https://prtg.example.com/{a}&username=username&passhash=12345678").ToArray(), true);
#pragma warning restore 618

            var client = Initialize_Client(response, RequestVersion.v18_1);

            await client.GetVersionClient(new object[] { property }).SetChannelPropertyAsync(new[] { 1001 }, 1, null, new[] { new ChannelParameter(property, val) }, CancellationToken.None);

            response.AssertFinished();
        }

        private async Task SetChannelPropertyAsync(Tuple<ChannelProperty, object, int?[][]> config, RequestVersion version, string[] addresses)
        {
            var property = config.Item1;
            var val = config.Item2;
            var matrix = config.Item3;

            var channels = matrix.Select(CreateChannel).ToList();

#pragma warning disable 618
            var response = new AddressValidatorResponse(addresses.Select(a => $"https://prtg.example.com/editsettings?{a}&username=username&passhash=12345678").ToArray(), true);
#pragma warning restore 618

            var client = Initialize_Client(response, version);

            await client.GetVersionClient(new object[] { property }).SetChannelPropertyAsync(channels.Select(c => c.SensorId).ToArray(), 2, channels, new[] { new ChannelParameter(property, val) }, CancellationToken.None);
        }

        #endregion
    }
}
