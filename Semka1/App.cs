﻿using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;

namespace Semka1
{
    public class App
    {
        private T23Tree<KeyStr, Osoba> _strOsobyPcrDatum = new T23Tree<KeyStr, Osoba>();
        private T23Tree<KeyDat, PcrTest> _strPcrDatum = new T23Tree<KeyDat, PcrTest>();//data is saved here... ostatne su iba ref
        private T23Tree<KeyInt, PcrTest> _strPcrID = new T23Tree<KeyInt, PcrTest>();
        private T23Tree<KeyInt, Miesto> _strKrajPcrDatum = new T23Tree<KeyInt, Miesto>();
        private T23Tree<KeyInt, Miesto> _strOkresPcrDatum = new T23Tree<KeyInt, Miesto>();
        private T23Tree<KeyInt, Miesto> _strPracoviskoPcrDatum = new T23Tree<KeyInt, Miesto>();
        //private List<KeyInt> _pcrIdList = new List<KeyInt>();
        private List<string> _rodCisList = new List<string>();
        public App()
        {
        }
        public bool O01_VlozPCR(int pKodPracoviska, int pKodOkresu, int pKodKraja, string pRodCislo,
            DateTime pDatTestu, bool pVyslTestu, string pPoznamka)
        {
            //check if rod_cislo is in database
            if (!_strOsobyPcrDatum.Contains(new KeyStr(pRodCislo))) { return false; }
            //generate unique pcr ID
            var rand = new Random();
            int kodPcr = -1;
            do {
                kodPcr = rand.Next(1, Int32.MaxValue); //cisla id pcr testu od 1 po Int32.MaxValue
            } while (_strPcrID.Contains(new KeyInt(kodPcr)));
            //_pcrIdList.Add(new KeyInt(kodPcr));
            //create test and insert everywhere
            var pcrTest = new PcrTest(pKodPracoviska,kodPcr,pKodOkresu,pKodKraja,pRodCislo, _strOsobyPcrDatum.GetData(new KeyStr(pRodCislo)), pDatTestu, pVyslTestu,pPoznamka);
            _strPcrDatum.Insert(new KeyDat(pDatTestu, kodPcr), pcrTest);
            //ref var pcrTestRef = ref _strPcrDatum.GetDataRef(new KeyDat(pDatTestu, kodPcr));
            _strPcrID.Insert(new KeyInt(kodPcr), pcrTest);
            _strOsobyPcrDatum.GetData(new KeyStr(pRodCislo)).GetTree().Insert(new KeyDat(pDatTestu, kodPcr), pcrTest);
            if (_strKrajPcrDatum.Contains(new KeyInt(pKodKraja))) 
                _strKrajPcrDatum.GetDataRef(new KeyInt(pKodKraja)).GetTree().Insert(new KeyDat(pDatTestu, kodPcr), pcrTest);
            else { _strKrajPcrDatum.Insert(new KeyInt(pKodKraja), new Miesto(pKodKraja));
                _strKrajPcrDatum.GetDataRef(new KeyInt(pKodKraja)).GetTree().Insert(new KeyDat(pDatTestu, kodPcr), pcrTest); }
            if (_strOkresPcrDatum.Contains(new KeyInt(pKodOkresu))) _strOkresPcrDatum.GetDataRef(
                new KeyInt(pKodOkresu)).GetTree().Insert(new KeyDat(pDatTestu, kodPcr), pcrTest);
            else { _strOkresPcrDatum.Insert(new KeyInt(pKodOkresu), new Miesto(pKodOkresu));
                _strOkresPcrDatum.GetDataRef(new KeyInt(pKodOkresu)).GetTree().Insert(new KeyDat(pDatTestu, kodPcr), pcrTest); }
            if (_strPracoviskoPcrDatum.Contains(new KeyInt(pKodPracoviska))) _strPracoviskoPcrDatum.GetDataRef(
                new KeyInt(pKodPracoviska)).GetTree().Insert(new KeyDat(pDatTestu, kodPcr), pcrTest);
            else { _strPracoviskoPcrDatum.Insert(new KeyInt(pKodPracoviska), new Miesto(pKodPracoviska));
                _strPracoviskoPcrDatum.GetDataRef(new KeyInt(pKodPracoviska)).GetTree().Insert(new KeyDat(pDatTestu, kodPcr), pcrTest); }
            return true;
        }
        public bool O02_VyhladajTest(int pCisloTestu, string pRodCislo, ref string textOut)
        {
            var testNaVypis = _strPcrID.GetData(new KeyInt(pCisloTestu));
            var osobaNaVypis = _strOsobyPcrDatum.GetData(new KeyStr(pRodCislo));
            if (testNaVypis == null || osobaNaVypis == null) return false;
            textOut = testNaVypis.ToString() + " " + osobaNaVypis.ToString();
            return true;
        }
        public bool O03_VypisVsetkyPcrOsoba(string pRodCislo, ref string vypis)
        {
            if (_strOsobyPcrDatum == null && _strPcrID == null) return false;
            if (_strOsobyPcrDatum == null) return false;
            var hladanaOsoba = _strOsobyPcrDatum.GetData(new KeyStr(pRodCislo));
            if (hladanaOsoba == null) return false;
            vypis += "pocet: 1\n" + hladanaOsoba.ToString();
            var zoznam = hladanaOsoba.GetTree().InOrder();
            vypis += "pocet: " + zoznam.Count + "\n";
            foreach (var z in zoznam)
            {
                vypis += z.ToString();
            }
            return true;
        }
        public bool O04_VypisPozitivnychOkresDatum(int numOkresu, DateTime datOd, DateTime datDo, ref string vypis)
        {
            var okr = new KeyInt(numOkresu);
            if (!_strOkresPcrDatum.Contains(okr)) return false;
            var datOdKey = new KeyDat(datOd, 1);
            var datDoKey = new KeyDat(datDo, Int32.MaxValue);
            var zoznam = _strOkresPcrDatum.GetData(okr).GetTree().InOrder(datOdKey, datDoKey);
            var zozPoz = new List<PcrTest>();
            foreach (var z in zoznam)
            {
                if (z.IsPositive()) zozPoz.Add(z);
            }
            vypis += "pocet: " + zozPoz.Count + "\n";
            foreach (var z in zozPoz)
            {
                vypis += z.ToString();
            }
            return true;
        }
        public bool O05_VypisVsetkychOkresDatum(int numOkresu, DateTime datOd, DateTime datDo, ref string vypis)
        {
            var okr = new KeyInt(numOkresu);
            if (!_strOkresPcrDatum.Contains(okr)) return false;
            var datOdKey = new KeyDat(datOd, 1);
            var datDoKey = new KeyDat(datDo, Int32.MaxValue);
            var zoznam = _strOkresPcrDatum.GetData(okr).GetTree().InOrder(datOdKey, datDoKey);
            vypis += "pocet: " + zoznam.Count + "\n";
            foreach (var z in zoznam)
            {
                vypis += z.ToString();
            }
            return true;
        }
        public bool O06_VypisPozitivnychKrajDatum(int numKraja, DateTime datOd, DateTime datDo, ref string vypis)
        {
            var kraj = new KeyInt(numKraja);
            if (!_strKrajPcrDatum.Contains(kraj)) return false;
            var datOdKey = new KeyDat(datOd, 1);
            var datDoKey = new KeyDat(datDo, Int32.MaxValue);
            var zoznam = _strKrajPcrDatum.GetData(kraj).GetTree().InOrder(datOdKey, datDoKey);
            var zozPoz = new List<PcrTest>();
            foreach (var z in zoznam)
            {
                if (z.IsPositive()) zozPoz.Add(z);
            }
            vypis += "pocet: " + zozPoz.Count + "\n";
            foreach (var z in zozPoz)
            {
                vypis += z.ToString();
            }
            return true;
        }
        public bool O07_VypisVsetkychKrajDatum(int numKraja, DateTime datOd, DateTime datDo, ref string vypis)
        {
            var kraj = new KeyInt(numKraja);
            if (!_strKrajPcrDatum.Contains(kraj)) return false;
            var datOdKey = new KeyDat(datOd, 1);
            var datDoKey = new KeyDat(datDo, Int32.MaxValue);
            var zoznam = _strKrajPcrDatum.GetData(kraj).GetTree().InOrder(datOdKey, datDoKey);
            vypis += "pocet: " + zoznam.Count + "\n";
            foreach (var z in zoznam)
            {
                vypis += z.ToString();
            }
            return true;
        }
        public bool O08_VypisVsetkychPozitivnychTestovDatum(DateTime datOd, DateTime datDo, ref string vypis)
        {
            if (_strPcrDatum == null) return false;
            var datOdKey = new KeyDat(datOd, 1);
            var datDoKey = new KeyDat(datDo, Int32.MaxValue);
            var zoznam = _strPcrDatum.InOrder(datOdKey, datDoKey);
            var zozPoz = new List<PcrTest>();
            foreach (var z in zoznam)
            {
                if (z.IsPositive()) zozPoz.Add(z);
            }
            vypis += "pocet: " + zozPoz.Count + "\n";
            foreach (var z in zozPoz)
            {
                vypis += z.ToString();
            }
            return true;
        }
        public bool O09_VypisVsetkychTestovDatum(DateTime datOd, DateTime datDo, ref string vypis)
        {
            if (_strPcrDatum == null) return false;
            var datOdKey = new KeyDat(datOd, 1);
            var datDoKey = new KeyDat(datDo, Int32.MaxValue);
            var zoznam = _strPcrDatum.InOrder(datOdKey, datDoKey);
            vypis += "pocet: " + zoznam.Count + "\n";
            foreach (var z in zoznam)
            {
                vypis += z.ToString();
            }
            return true;
        }
        public bool O15_VypisVsetkychPracDatum(int numPracoviska, DateTime datOd, DateTime datDo, ref string vypis)
        {
            var pracKey = new KeyInt(numPracoviska);
            if (!_strPracoviskoPcrDatum.Contains(pracKey)) return false;
            var datOdKey = new KeyDat(datOd, 1);
            var datDoKey = new KeyDat(datDo, Int32.MaxValue);
            var zoznam = _strPracoviskoPcrDatum.GetData(pracKey).GetTree().InOrder(datOdKey, datDoKey);
            vypis += "pocet: " + zoznam.Count + "\n";
            foreach (var z in zoznam)
            {
                vypis += z.ToString();
            }
            return true;
        }
        public bool O16_VyhladajTest(int pCisloTestu, ref string textOut)
        {
            var testNaVypis = _strPcrID.GetData(new KeyInt(pCisloTestu));
            if (testNaVypis == null) return false;
            textOut += testNaVypis.ToString();
            return true;
        }
        public bool O17_VlozOsobu(string pMeno, string pPriezvisko, string pRodCislo, DateTime pDatNar)
        {
            if (_strOsobyPcrDatum.Insert(new KeyStr(pRodCislo), new Osoba(pMeno, pPriezvisko, pRodCislo, pDatNar)))
            { _rodCisList.Add(pRodCislo); return true; }
            return false;
        }
        public bool O18_VymazPcrTest(int pCisloTestu)
        {//vymaze pcr test zo vsetkych stromov
            if(!_strPcrID.Contains(new KeyInt(pCisloTestu))) return false;
            var t = _strPcrID.GetData(new KeyInt(pCisloTestu));
            var keyPcrDat = new KeyDat(t.GetDat(), pCisloTestu);
            _strKrajPcrDatum.GetData(new KeyInt(t.GetKraj())).GetTree().Delete(keyPcrDat);
            _strOkresPcrDatum.GetData(new KeyInt(t.GetOkres())).GetTree().Delete(keyPcrDat);
            _strPracoviskoPcrDatum.GetData(new KeyInt(t.GetPrac())).GetTree().Delete(keyPcrDat);
            _strOsobyPcrDatum.GetData(new KeyStr(t.GetRodCis())).GetTree().Delete(keyPcrDat);
            _strPcrID.Delete(new KeyInt(t.GetKod()));
            _strPcrDatum.Delete(keyPcrDat);
            return true;
        }
        public bool O19_VymazOsobu(string pRodCislo)
        {//vymaze osobu a vsetky jej testy zo systemu
            var keyOsoba = new KeyStr(pRodCislo);
            if (!_strOsobyPcrDatum.Contains(keyOsoba)) return false;
            var pcrTesty = _strOsobyPcrDatum.GetData(keyOsoba).GetTree().InOrder();
            foreach (var t in pcrTesty)
            {
                var keyPcrDat = new KeyDat(t.GetDat(), t.GetKod());
                _strKrajPcrDatum.GetData(new KeyInt(t.GetKraj())).GetTree().Delete(keyPcrDat);
                _strOkresPcrDatum.GetData(new KeyInt(t.GetOkres())).GetTree().Delete(keyPcrDat);
                _strPracoviskoPcrDatum.GetData(new KeyInt(t.GetPrac())).GetTree().Delete(keyPcrDat);
                _strPcrID.Delete(new KeyInt(t.GetKod()));
                _strPcrDatum.Delete(keyPcrDat);
            }
            _strOsobyPcrDatum.Delete(keyOsoba);
            _rodCisList.Remove(pRodCislo);
            return true;
        }
        public bool GenerateData(int pNumOfPeople, int pNumOfTests)
        {
            if (_strOsobyPcrDatum.Count() == 0 && pNumOfPeople == 0) return false;
            var rand = new Random();
            var firstNames = new string[]{
                "Noah" ,"Liam" ,"William" ,"Mason" , "James" ,"Benjamin" ,"Jacob" , "Michael" ,"Elijah" ,"Ethan" ,
                "Alexander" ,"Oliver" ,"Daniel" , "Lucas" ,"Matthew" ,"Aiden" , "Jackson" ,"Logan" ,"David" ,"Joseph" ,
                "Samuel" ,"Henry" ,"Owen" ,"Sebastian" , "Gabriel" ,"Carter" ,"Jayden" ,"John" , "Luke" ,"Anthony",
                "Olivia", "Emma", "Ava", "Charlotte", "Sophia", "Amelia", "Isabella", "Lucas", "Mia", "Henry", "Evelyn", "Harper"};
            var lastNames = new string[]{"Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez",
                "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson",
                "Martin", "Lee", "Perez", "Thompson", "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson",
                "Walker", "Young", "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores", "Green", " Adams",
                "Nelson", "Baker", "Hall", "Rivera", "Campbell", "Mitchell", "Carter", "Roberts" };
            for (int i = 0; i < pNumOfPeople; i++)
            {
                string cislotry;
                do
                {
                    cislotry = rand.Next(1, Int32.MaxValue).ToString("0000000000"); //rodne cisla od 1 po Int32.MaxValue
                } while (_strOsobyPcrDatum.Contains(new KeyStr(cislotry)));
                _rodCisList.Add(cislotry);
                _strOsobyPcrDatum.Insert(new KeyStr(cislotry), new Osoba(firstNames[rand.Next(firstNames.Length)],
                    lastNames[rand.Next(lastNames.Length)], cislotry, rand.Next(1900, 2021),
                    rand.Next(1, 13), rand.Next(1, 29)));
            }
            for (int i = 0; i < pNumOfTests; i++)
            {
                int pcrID = 0;
                var pcrIDkey = new KeyInt(pcrID);
                do
                {
                    pcrID = rand.Next(1, Int32.MaxValue); //cisla id pcr testu od 1 po Int32.MaxValue
                    pcrIDkey = new KeyInt(pcrID);
                } while (_strPcrID.Contains(pcrIDkey));
                //_pcrIdList.Add(pcrIDkey);
                var aktRodCis = new KeyStr(_rodCisList[rand.Next(_strOsobyPcrDatum.Count())]);
                var aktDatum = new KeyDat(new DateTime(DateTime.Now.Ticks).AddDays(-rand.Next(600))
                    .AddHours(-rand.Next(24)).AddMinutes(-rand.Next(60)).AddSeconds(-rand.Next(60)), pcrID);
                var prac = rand.Next(1, 151); // 1-150
                var pracKey = new KeyInt(prac);
                var okres = rand.Next(1, 80); // 1-79
                var okresKey = new KeyInt(okres);
                var kraj = rand.Next(1, 9); // 1-8
                var krajKey = new KeyInt(kraj);
                //insert everywhere
                var pcrTest = new PcrTest(prac, pcrID, okres, kraj,
                    aktRodCis.GetKey(), _strOsobyPcrDatum.GetData(aktRodCis), aktDatum.GetKey(), rand.Next(2) == 0, "");
                _strPcrDatum.Insert(aktDatum, pcrTest);
                //ref var pcrTestRef = ref _strPcrDatum.GetData(aktDatum);
                _strOsobyPcrDatum.GetData(aktRodCis).GetTree().Insert(aktDatum, pcrTest);
                _strPcrID.Insert(pcrIDkey, pcrTest);
                if (_strKrajPcrDatum.Contains(krajKey))
                    _strKrajPcrDatum.GetData(krajKey).GetTree().Insert(aktDatum, pcrTest);
                else
                {
                    _strKrajPcrDatum.Insert(krajKey, new Miesto(kraj));
                    _strKrajPcrDatum.GetData(krajKey).GetTree().Insert(aktDatum, pcrTest);
                }
                if (_strOkresPcrDatum.Contains(okresKey))
                    _strOkresPcrDatum.GetData(okresKey).GetTree().Insert(aktDatum, pcrTest);
                else
                {
                    _strOkresPcrDatum.Insert(okresKey, new Miesto(okres));
                    _strOkresPcrDatum.GetData(okresKey).GetTree().Insert(aktDatum, pcrTest);
                }
                if (_strPracoviskoPcrDatum.Contains(pracKey))
                    _strPracoviskoPcrDatum.GetData(pracKey).GetTree().Insert(aktDatum, pcrTest);
                else
                {
                    _strPracoviskoPcrDatum.Insert(pracKey, new Miesto(prac));
                    _strPracoviskoPcrDatum.GetData(pracKey).GetTree().Insert(aktDatum, pcrTest);
                }
            }
            return true;
        }
        public string VypisVsetko()
        {
            if (_strOsobyPcrDatum == null && _strPcrID == null)
                return "";
            if (_strOsobyPcrDatum == null)
                return _strPcrID.VypisVsetko();
            if (_strPcrID == null)
                return _strOsobyPcrDatum.VypisVsetko();
            return _strOsobyPcrDatum.VypisVsetko() + _strPcrID.VypisVsetko();
        }


    }
}