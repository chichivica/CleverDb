using CleverApi.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft;
using System.Web.Helpers;
using Newtonsoft.Json;

namespace CleverApi.Tests
{
    [TestClass]
    public class TestCleverDbController
    {
        [TestMethod]
        public void Insert_ShouldReturnCleverObject()
        {
            var controller = new CleverDbController();
            var json = @"{
                        'name' : 'Насос',
                        'parentId' : 95,
                        'attributes' : {
                                           'марка' : 'Bosh',
                                            'мощность' : 95.3,
                                            'ДатаУстановки' : new Date(1224043200000)
                                        }
                        }";

            //var obj = new
            //{
            //    name = "Насос",
            //    parentId = 95,
            //    attributes = new
            //    {
            //        марка = "Bosh",
            //        мощность = 95.3,
            //        ДатаУстановки = DateTime.Now
            //    }
            //};

            //var str = Json.Encode(obj);
            //dynamic hmm = Json.Decode(str);

            dynamic newt= JsonConvert.DeserializeObject<dynamic>(json);
            dynamic result = controller.Insert(newt);


            Assert.IsInstanceOfType(result.Content.Id, typeof(int));
            Assert.AreNotEqual<int>(result.Content.Id, 0);
        }
    }
}
