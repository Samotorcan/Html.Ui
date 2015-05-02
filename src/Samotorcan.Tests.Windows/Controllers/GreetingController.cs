﻿using Samotorcan.HtmlUi.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samotorcan.Tests.Windows.Controllers
{
    /// <summary>
    /// Greeting controller.
    /// </summary>
    public class GreetingController : ObservableController
    {
        public int FirstNumber { get; set; }


        private int _secondNumber;
        public int SecondNumber
        {
            get { return _secondNumber; }
            set { SetField(ref _secondNumber, value); }
        }

        private int _result;
        public int Result
        {
            get { return _result; }
            set { SetField(ref _result, value); }
        }

        private ObservableCollection<string> _someList;
        public ObservableCollection<string> SomeList
        {
            get { return _someList; }
            set { SetField(ref _someList, value); }
        }

        public int[] SomeArray { get; set; }

        public GreetingController()
        {
            FirstNumber = 123;

            SomeList = new ObservableCollection<string>(new List<string> { "123", "234", "345" });
            SomeList.Add("test");
            SomeList.Add("test2");
            SomeList.Add("test2");
            SomeList.Add("test3");

            SomeList.Move(0, 2);
            SomeList.RemoveAt(0);
            SomeList.Remove("test3");

            SomeList.Insert(1, "tt");

            SomeArray = new int[10];
        }

        public void Sum()
        {
            Result = FirstNumber + SecondNumber;
        }

        public void Sum1()
        {
            var random = new Random();

            SomeList[2] = "55" + random.Next();
        }
    }
}
