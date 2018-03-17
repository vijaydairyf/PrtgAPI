﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.Parameters;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Creates a new set of sensor parameters for creating a brand new sensor under a device.</para>
    /// 
    /// <para type="description">The New-SensorParameters cmdlet creates a set of parameters for adding a brand
    /// new sensor to PRTG. All sensor types supported by PRTG can be added with New-SensorParameters, however certain
    /// types are "natively" supported.</para>
    /// 
    /// <para type="description">Natively supported sensor types allow interfacing with strongly typed properties
    /// of a well known object deriving from NewSensorParameters. When a supported type is created, the name to give the
    /// sensor can be optionally specified. If a name is not specified, New-SensorParameters will automatically assign
    /// the sensor the default name PRTG would assign a sensor of the specified type (e.g. EXE/Script Advanced sensors
    /// by default are named "XML Custom EXE/Script Sensor").</para>
    /// 
    /// <para type="description">In addition to the sensor name, certain sensor types contain additional mandatory fields
    /// that must be populated before attempting to add the sensor (such as the ExeFile of an EXE/Script Advanced sensor).
    /// New-SensorParameters optionally allows you to specify the value of the primary mandatory field of the specified
    /// type using the -Value parameter. Fields that require values contain a value of $null by default, however not all
    /// fields that are $null are necessarily mandatory. If you attempt to add a natively supported type with missing
    /// mandatory fields, PrtgAPI will catch this and alert you that the value that was missing.</para>
    /// 
    /// <para type="description">Beyond type safety, PrtgAPI does not perform any validation
    /// that the values you specify to fields are "correct" (e.g. for an EXE/Script Advanced sensor, that the specified
    /// file exists). In the event invalid values are specified, PRTG will usually handle the error gracefully, however you
    /// are responsible for confirming that any values that are used to create a new sensor as are as correct as possible.</para>
    /// 
    /// <para type="description">For sensor types that are not supported by PrtgAPI, these sensors can still be added by
    /// defining a hashtable listing all the parameters that are required to create the specified sensor, along with their
    /// raw associated values. The parameters for creating a specific type of sensor can be discovered via a web debugger
    /// such as Fiddler.</para>
    /// 
    /// <para type="description">Great care should be taken when adding sensors using raw parameters. As there is no type safety,
    /// the possibility of making errors is high. As most raw parameter names end in an underscore, it is critical to ensure
    /// these parameters have been named properly. In the event a sensor is added improperly, it can easily be corrected or
    /// deleted in the PRTG UI. When specifying a hashtable parameter set to New-SensorParameters, PrtgAPI will validate
    /// that at a minimum the 'name_' and 'sensortype' parameters are specified. If either of these two are missing,
    /// New-SensorParameters will generate an exception.</para>
    /// 
    /// <example>
    ///     <code>C:\> $params = New-SensorParameters ExeXml "Custom Script" "CustomScript.ps1"</code>
    ///     <para>C:\> Get-Device dc-1 | Add-Sensor $params</para>
    ///     <para>Create a new EXE/Script Advanced sensor on the device dc-1 using the name "Custom Script", that executes the file "CustomScript.ps1", specifying the script name in the optional -Value parameter</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> $params = New-SensorParameters ExeXml "Custom Script"</code>
    ///     <para>C:\> $params.ExeFile = "CustomScript.ps1"</para>
    ///     <para>C:\> Get-Device dc-1 | Add-Sensor $params</para>
    ///     <para>Create a new EXE/Script Advanced sensor on the device dc-1 using the name "Custom Script", that executes the file "CustomScript.ps1", specifying the script name after the object has been created</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> $params = New-SensorParameters ExeXml</code>
    ///     <para>C:\> $params.ExeFile = "CheckStatus.ps1"</para>
    ///     <para>C:\> Get-Device -Id 1001 | Add-Sensor $params</para>
    ///     <para>Create a new EXE/Script Advanced sensor on the device with ID 1001 using the name "XML Custom EXE/Script Sensor" that executes the file "CheckStatus.ps1"</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> $raw = @{</code>
    ///     <para>>>     name_ = "my raw sensor"</para>
    ///     <para>>>     tags_ = "xmlexesensor"</para>
    ///     <para>>>     priority_ = 4</para>
    ///     <para>>>     exefile_ = "CustomScript.ps1|CustomScript.ps1||</para>
    ///     <para>>>     exeparams_ = "arg1 arg2 arg3"</para>
    ///     <para>>>     environment_ = 1</para>
    ///     <para>>>     usewindowsauthentication_ = 1</para>
    ///     <para>>>     mutexname_ = "testMutex"</para>
    ///     <para>>>     timeout_ = 70</para>
    ///     <para>>>     writeresult_ = 1</para>
    ///     <para>>>     intervalgroup = 0</para>
    ///     <para>>>     interval_ = "30|30 seconds"</para>
    ///     <para>>>     errorintervalsdown_ = 2</para>
    ///     <para>>>     sensortype = "exexml"</para>
    ///     <para>>> }</para>
    ///     <para>C:\> $params = New-SensorParameters $raw</para>
    ///     <para>C:\> Get-Device dc-1 | Add-Sensor $params</para>
    ///     <para>Add a new EXE/Script Advanced sensor to the device named dc-1 using its raw parameters</para>
    /// </example>
    /// 
    /// <para type="link">about_SensorParameters</para>
    /// <para type="link">Add-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// </summary>
    [Cmdlet(VerbsCommon.New, "SensorParameters", DefaultParameterSetName = ParameterSet.Default)]
    public class NewSensorParametersCommand : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The type of sensor to create.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Default, Position = 0)]
        public SensorType Type { get; set; }

        /// <summary>
        /// <para type="description">The name to give the new sensor. If no value is specified, the default name of the specified sensor type will be used.
        /// If the specified sensor type does not support specifying a name, this field is used for any mandatory values required by the sensor type.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Default, Position = 1)]
        public object First { get; set; }

        /// <summary>
        /// <para type="description">A mandatory value required by the specified sensor type.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Default, Position = 2)]
        public object Second { get; set; }

        /// <summary>
        /// <para type="description">A collection of raw parameters for adding an unsupported sensor type.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Raw, Position = 0)]
        public Hashtable RawParameters { get; set; }

        private bool ignoreName;

        private const string NAME_PARAMETER = "name_";
        private const string SENSORTYPE_PARAMETER = "sensortype";

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            var parameters = ParameterSetName == "Raw" ? CreateRawParameters() : CreateTypedParameters();

            if (First != null && !ignoreName)
                parameters.Name = First.ToString();

            WriteObject(parameters);
        }

        private NewSensorParameters CreateRawParameters()
        {
            if (!RawParameters.ContainsKey(NAME_PARAMETER))
                throw new InvalidOperationException($"Hashtable record '{NAME_PARAMETER}' is mandatory, however a value was not specified");

            if (!RawParameters.ContainsKey(SENSORTYPE_PARAMETER))
                throw new InvalidOperationException($"Hashtable record '{SENSORTYPE_PARAMETER}' is mandatory, however a value was not specified'");

            var parameters = new RawSensorParameters(RawParameters[NAME_PARAMETER]?.ToString(), RawParameters[SENSORTYPE_PARAMETER]?.ToString())
            {
                Parameters = RawParameters.Keys.Cast<object>().Where(k => k.ToString() != SENSORTYPE_PARAMETER).Select(k => new CustomParameter(k.ToString(), RawParameters[k])).ToList()
            };

            return parameters;
        }

        private NewSensorParameters CreateTypedParameters()
        {
            NewSensorParameters parameters;

            switch (Type)
            {
                case SensorType.ExeXml:
                    parameters = new ExeXmlSensorParameters(string.Empty) { ExeFile = GetImplicit<ExeFileTarget>(Second) };
                    break;
                case SensorType.WmiService:
                    ignoreName = true;
                    parameters = new WmiServiceSensorParameters(new List<WmiServiceTarget>()) { Services = GetList<WmiServiceTarget>(First) };
                    break;
                default:
                    throw new NotImplementedException($"Sensor type '{Type}' is currently not supported");
            }

            return parameters;
        }

        [ExcludeFromCodeCoverage]
        private T GetImplicit<T>(object val)
        {
            if (val == null)
                return default(T);

            if (val is T)
                return (T) val;

            var implicitOp = typeof (T).GetMethod("op_Implicit", new[] {typeof (string)});

            if (implicitOp == null)
                throw new InvalidOperationException($"Object type {typeof (T)} does not contain an implicit operator for objects of type string");

            return (T) implicitOp.Invoke(null, new object[] {val.ToString()});
        }

        [ExcludeFromCodeCoverage]
        private List<T> GetList<T>(object val)
        {
            if (val == null)
                return null;

            if (val is PSObject)
                val = ((PSObject) val).BaseObject;

            if (val is T)
                return new List<T> {(T)val };

            if (val is List<T>)
                return (List<T>)val;

            if (val is IEnumerable)
            {
                var objList = ((IEnumerable) val).Cast<object>().Where(o => o != null);

                var list = new List<T>();

                foreach (var obj in objList)
                {
                    var obj1 = obj;

                    if (obj1 is PSObject)
                        obj1 = ((PSObject) obj).BaseObject;

                    if (obj1 is T)
                        list.Add((T)obj1);
                    else
                        throw new ArgumentException($"Expected one or more items of type {typeof (T)}, however an item of type {obj1.GetType()} was specified");
                }

                return list;
            }

            throw new ArgumentException($"Expected one or more items of type {typeof (T)}, however an item of type {val.GetType()} was specified");
        }
    }
}
