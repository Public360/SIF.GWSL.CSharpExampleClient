using CaseService = SIF.GWL.ClientExample.CaseService;
using ContactService = SIF.GWL.ClientExample.ContactService;
using DocumentService = SIF.GWL.ClientExample.DocumentService;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIF.GWSL.ExampleClient
{
    class Program
    {

        private static string GwslUser = ConfigurationManager.AppSettings["GwslUser"];
        private static string GwslPassword = ConfigurationManager.AppSettings["GwslPassword"];
        
        static void Main(string[] args)
        {
            try
            {
                ContactService_QuickAndDirtyTestMethod();
                DocumentService_QuickAndDirtyTestMethod();
                CaseService_QuickAndDirtyTestMethod();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.ReadKey();
        }

        private static void ContactService_QuickAndDirtyTestMethod()
        {

            var contactClient = new ContactService.ContactServiceClient("BasicHttpBinding_IContactService");
            contactClient.ClientCredentials.UserName.UserName = GwslUser;
            contactClient.ClientCredentials.UserName.Password = GwslPassword;

            //Idempotent operation, meaning that the state of the contact in 360 is unchanged no matter how many times the same request is repeated.
            var createContactResult = contactClient.SynchronizePrivatePerson(new ContactService.SynchronizePrivatePersonParameter()
            {
                //ADContextUser = "domain\\someuser",   //Add to run as a different user than the integration user in 360.
                ExternalID = "24079026111", //Your external ID which can be used for finding the same contact later. Can be any unique ID.
                PersonalIdNumber = "24079026111",
                FirstName = "Testfirstname",
                LastName = "Testlastname",
                Email = "tester@test.no",
                MobilePhone = "+4711223344",
                PostAddress = new ContactService.Address()
                {
                    StreetAddress = "Monrads gate 21B",
                    ZipCode = "0564",
                    ZipPlace = "Oslo",
                    Country = "Norge"
                }
            });

            Console.WriteLine("Create contact result: \n" + Newtonsoft.Json.JsonConvert.SerializeObject(createContactResult));

            var getContactsResult = contactClient.GetPrivatePersons(new ContactService.GetPrivatePersonsParameter()
            {
                //ADContextUser = "domain\\someuser",   //Add to run as a different user than the integration user in 360.
                ExternalID = "24079026111" //or PersonalIdNumber = "24079026111"
            });

            if (getContactsResult.Successful)
            {
                Console.WriteLine("\nContacts ({0}):", getContactsResult.PrivatePersons.Length);
                foreach (var privatePerson in getContactsResult.PrivatePersons)
                {
                    Console.WriteLine(privatePerson.ExternalID + ": " + privatePerson.FirstName + " " + privatePerson.LastName);
                }
            }
        }

        private static void DocumentService_QuickAndDirtyTestMethod()
        {
            var documentClient = new DocumentService.DocumentServiceClient("BasicHttpBinding_IDocumentService");
            documentClient.ClientCredentials.UserName.UserName = GwslUser;
            documentClient.ClientCredentials.UserName.Password = GwslPassword;

            var result1 = documentClient.CreateDocument(new DocumentService.CreateDocumentParameter()
            {
                ADContextUser = "domain\\someuser",
                Title = "Inkassovarsel sendt",
                Category = "recno:111",
                DocumentDate = DateTime.Now,
                Archive = "recno:2",
                Status = "recno:6",
                CaseNumber = @"16/235923",
                Contacts = new DocumentService.DocumentContactParameter[]
                {
                    new DocumentService.DocumentContactParameter()
                    {
                        ReferenceNumber = "xxxxxxxxxxx",
                        Role = "recno: 6" //mottaker
                    }
                }
            });

            Console.WriteLine("Created new document with document number: " + result1.DocumentNumber);

            var result2 = documentClient.CreateDocument(new DocumentService.CreateDocumentParameter()
            {
                ADContextUser = "domain\\someuser",
                Title = "Inkassovarsel mottatt",
                Category = "recno:110",
                DocumentDate = DateTime.Now,
                Archive = "recno:2",
                Status = "recno:6",
                CaseNumber = @"16/235923",
                UnregisteredContacts = new DocumentService.UnregisteredContactParameter[]
                {
                    new DocumentService.UnregisteredContactParameter()
                    {
                            Role = "recno: 5", //avsender,
                            ContactCompanyName = "Testavsender"
                    }
                }
            });

            Console.WriteLine("Created new document with an Unregistered Contact, document number: " + result2.DocumentNumber);
        }

        private static void CaseService_QuickAndDirtyTestMethod()
        {
            var caseClient = new CaseService.CaseServiceClient("BasicHttpBinding_ICaseService");
            caseClient.ClientCredentials.UserName.UserName = GwslUser;
            caseClient.ClientCredentials.UserName.Password = GwslPassword;

            var createCaseResult = caseClient.CreateCase(new CaseService.CreateCaseParameter()
            {
                Title = "Testcase",
                ExternalId = new CaseService.ExternalIdParameter() { Id = "externalCaseNr1", Type = "Visma" }
            });

            Console.WriteLine("CREATE RESULT: \n" + Newtonsoft.Json.JsonConvert.SerializeObject(createCaseResult));

            var getCaseResult = caseClient.GetCases(new CaseService.GetCasesQuery()
            {
                ExternalId = new CaseService.ExternalIdParameter() { Id = "externalCaseNr1", Type = "Visma" }
            });

            Console.WriteLine("GET RESULT: \n" + Newtonsoft.Json.JsonConvert.SerializeObject(getCaseResult));

            Console.WriteLine("COUNT: " + getCaseResult.Cases.Length);
        }
    }
}