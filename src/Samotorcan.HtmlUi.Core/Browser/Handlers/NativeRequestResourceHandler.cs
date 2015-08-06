﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Samotorcan.HtmlUi.Core.Exceptions;
using Samotorcan.HtmlUi.Core.Logs;
using Samotorcan.HtmlUi.Core.Messages;
using Samotorcan.HtmlUi.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using Xilium.CefGlue;

namespace Samotorcan.HtmlUi.Core.Browser.Handlers
{
    /// <summary>
    /// Native request resource handler.
    /// </summary>
    internal class NativeRequestResourceHandler : CefResourceHandler
    {
        #region Properties
        #region Private

        #region Url
        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        private string Url { get; set; }
        #endregion
        #region Path
        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path { get; set; }
        #endregion
        #region Exception
        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        /// <value>
        /// The exception.
        /// </value>
        private Exception Exception { get; set; }
        #endregion
        #region Data
        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        private byte[] Data { get; set; }
        #endregion
        #region ResponseValue
        /// <summary>
        /// Gets or sets the response value.
        /// </summary>
        /// <value>
        /// The response value.
        /// </value>
        private object ResponseValue { get; set; }
        #endregion
        #region AllBytesRead
        /// <summary>
        /// Gets or sets all bytes read.
        /// </summary>
        /// <value>
        /// All bytes read.
        /// </value>
        private int AllBytesRead { get; set; }
        #endregion
        #region NativeFunctions
        /// <summary>
        /// Gets or sets the native functions.
        /// </summary>
        /// <value>
        /// The native functions.
        /// </value>
        private Dictionary<string, Func<CefRequest, object>> NativeFunctions { get; set; }
        #endregion

        #endregion
        #endregion
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeRequestResourceHandler"/> class.
        /// </summary>
        public NativeRequestResourceHandler()
            : base()
        {
            NativeFunctions = NativeFunctionAttribute.GetMethods<NativeRequestResourceHandler, Func<CefRequest, object>>(this);
        }

        #endregion
        #region Methods
        #region Protected

        #region CanGetCookie
        /// <summary>
        /// Cookies are disabled, returns false.
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        protected override bool CanGetCookie(CefCookie cookie)
        {
            return false;
        }
        #endregion
        #region CanSetCookie
        /// <summary>
        /// Cookies are disabled, returns false.
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        protected override bool CanSetCookie(CefCookie cookie)
        {
            return false;
        }
        #endregion
        #region Cancel
        /// <summary>
        /// Request processing has been canceled.
        /// </summary>
        protected override void Cancel() { }
        #endregion
        #region GetResponseHeaders
        /// <summary>
        /// Gets the response headers.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="responseLength"></param>
        /// <param name="redirectUrl"></param>
        protected override void GetResponseHeaders(CefResponse response, out long responseLength, out string redirectUrl)
        {
            if (response == null)
                throw new ArgumentNullException("response");

            redirectUrl = null;

            response.Status = 200;
            response.StatusText = "OK";
            response.MimeType = "application/json";
            response.SetHeaderMap(new NameValueCollection { { "Access-Control-Allow-Origin", "*" } });

            var nativeResponse = new NativeResponse();

            // exception
            if (Exception != null)
            {
                nativeResponse.Type = NativeResponseType.Exception;
                nativeResponse.Value = null;
                nativeResponse.Exception = ExceptionUtility.CreateJavascriptException(Exception);
            }

            // ok
            else
            {
                if (ResponseValue == Value.Undefined)
                {
                    nativeResponse.Type = NativeResponseType.Undefined;
                    nativeResponse.Value = null;
                }
                else
                {
                    nativeResponse.Type = NativeResponseType.Value;
                    nativeResponse.Value = ResponseValue;
                }

                nativeResponse.Exception = null;
            }

            Data = JsonUtility.SerializeToByteJson(nativeResponse);
            responseLength = Data.Length;
        }
        #endregion
        #region ProcessRequest
        /// <summary>
        /// Processes the request for the view.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        protected override bool ProcessRequest(CefRequest request, CefCallback callback)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            if (callback == null)
                throw new ArgumentNullException("callback");

            Url = request.Url;
            Path = Application.Current.GetNativeRequestPath(Url);

            Logger.Debug(string.Format("Native request: {0}", Url));

            try
            {
                var nativeMethod = FindNativeMethod(Path);

                if (nativeMethod != null)
                    ResponseValue = nativeMethod(request);
                else
                    Exception = new NativeNotFoundException(Path);
            }
            catch (Exception e)
            {
                ResponseValue = null;
                Exception = e;
            }

            callback.Continue();

            return true;
        }
        #endregion
        #region ReadResponse
        /// <summary>
        /// Reads the response.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="bytesToRead"></param>
        /// <param name="bytesRead"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        protected override bool ReadResponse(Stream response, int bytesToRead, out int bytesRead, CefCallback callback)
        {
            if (response == null)
                throw new ArgumentNullException("response");

            bytesRead = 0;

            if (AllBytesRead >= Data.Length)
                return false;

            bytesRead = Math.Min(bytesToRead, Data.Length - AllBytesRead);
            response.Write(Data, AllBytesRead, bytesRead);

            AllBytesRead += bytesRead;

            return true;
        }
        #endregion

        #endregion
        #region Private

        #region GetControllerNames
        /// <summary>
        /// Gets the controller names.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "request", Justification = "It has to match to the delegate.")]
        [NativeFunction]
        private object GetControllerNames(CefRequest request)
        {
            List<string> controllerNames = null;

            Application.Current.InvokeOnMain(() =>
            {
                controllerNames = Application.Current.GetControllerNames();
            });

            return controllerNames;
        }
        #endregion
        #region CreateController
        /// <summary>
        /// Creates the controller.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [NativeFunction]
        private object CreateController(CefRequest request)
        {
            var controllerData = GetPostData<CreateController>(request);
            ControllerDescription controllerDescription = null;

            Application.Current.InvokeOnMain(() =>
            {
                var controller = Application.Current.Window.CreateController(controllerData.Name);

                controllerDescription = controller.GetControllerDescription();

                // camel case method names
                foreach (var method in controllerDescription.Methods)
                    method.Name = StringUtility.CamelCase(method.Name);
            });

            return controllerDescription;
        }
        #endregion
        #region CreateObservableController
        /// <summary>
        /// Creates the observable controller.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [NativeFunction]
        private object CreateObservableController(CefRequest request)
        {
            var controllerData = GetPostData<CreateController>(request);
            ObservableControllerDescription observableControllerDescription = null;

            Application.Current.InvokeOnMain(() =>
            {
                var observableController = Application.Current.Window.CreateObservableController(controllerData.Name);

                observableControllerDescription = observableController.GetObservableControllerDescription();

                // camel case property and method names
                foreach (var property in observableControllerDescription.Properties)
                    property.Name = StringUtility.CamelCase(property.Name);

                foreach (var method in observableControllerDescription.Methods)
                    method.Name = StringUtility.CamelCase(method.Name);
            });

            return observableControllerDescription;
        }
        #endregion
        #region DestroyController
        /// <summary>
        /// Destroys the controller.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [NativeFunction]
        private object DestroyController(CefRequest request)
        {
            var controllerId = GetPostData<int>(request);

            Application.Current.InvokeOnMain(() =>
            {
                Application.Current.Window.DestroyController(controllerId);
            });

            return Value.Undefined;
        }
        #endregion
        #region SyncControllerChanges
        /// <summary>
        /// Synchronizes the controller changes.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [NativeFunction]
        private object SyncControllerChanges(CefRequest request)
        {
            var controllerChanges = GetPostData<List<ControllerChange>>(request);

            var application = Application.Current;
            application.InvokeOnMain(() =>
            {
                application.Window.SyncControllerChangesToNative(controllerChanges);
            });

            return Value.Undefined;
        }
        #endregion
        #region CallMethod
        /// <summary>
        /// Calls the method.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [NativeFunction]
        private object CallMethod(CefRequest request)
        {
            var methodData = GetPostData<CallMethod>(request);
            object response = null;

            Application.Current.InvokeOnMain(() =>
            {
                response = Application.Current.Window.CallMethod(methodData.Id, methodData.Name, methodData.Arguments, methodData.InternalMethod);
            });

            return response;
        }
        #endregion
        #region Log
        /// <summary>
        /// Logs the message.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [NativeFunction]
        private object Log(CefRequest request)
        {
            var jsonToken = GetPostJsonToken(request);

            var type = LogType.Parse((string)jsonToken["type"]);
            var messageType = LogMessageType.Parse((string)jsonToken["messageType"]);
            var message = jsonToken["message"].ToString();

            if (type == LogType.GeneralLog)
                Logger.Log(messageType, message);

            return Value.Undefined;
        }
        #endregion

        #region GetPostData
        /// <summary>
        /// Gets the post data.
        /// </summary>
        /// <typeparam name="TData">The type of the data.</typeparam>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        private TData GetPostData<TData>(CefRequest request)
        {
            var json = GetPostJson(request);

            return JsonConvert.DeserializeObject<TData>(json);
        }
        #endregion
        #region GetAnonymousPostData
        /// <summary>
        /// Gets the anonymous post data.
        /// </summary>
        /// <typeparam name="TData">The type of the data.</typeparam>
        /// <param name="request">The request.</param>
        /// <param name="anonymousObject">The anonymous object.</param>
        /// <returns></returns>
        private TData GetAnonymousPostData<TData>(CefRequest request, TData anonymousObject)
        {
            var json = GetPostJson(request);

            return JsonConvert.DeserializeAnonymousType(json, anonymousObject);
        }
        #endregion
        #region GetPostJson
        /// <summary>
        /// Gets the post json.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        private string GetPostJson(CefRequest request)
        {
            return Encoding.UTF8.GetString(request.PostData.GetElements()[0].GetBytes());
        }
        #endregion
        #region GetPostJsonToken
        /// <summary>
        /// Gets the post json token.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        private JToken GetPostJsonToken(CefRequest request)
        {
            var json = GetPostJson(request);

            return JToken.Parse(json);
        }
        #endregion
        #region FindNativeMethod
        /// <summary>
        /// Finds the native method.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
        private Func<CefRequest, object> FindNativeMethod(string methodName)
        {
            Func<CefRequest, object> nativeMethod = null;

            if (NativeFunctions.TryGetValue(methodName, out nativeMethod))
                return nativeMethod;

            if (NativeFunctions.TryGetValue(StringUtility.PascalCase(methodName), out nativeMethod))
                return nativeMethod;

            if (NativeFunctions.TryGetValue(StringUtility.CamelCase(methodName), out nativeMethod))
                return nativeMethod;

            return null;
        }
        #endregion

        #endregion
        #endregion
    }
}
