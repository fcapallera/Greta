using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CoreBot.Store.Entity
{
    public class Country : IIdentifiable
    {
        [XmlElement("id")]
        public override int Id { get; set; }

        private int? _zoneId { get; set; }

        [XmlElement("id_zone")]
        public int ZoneId
        {
            get { return _zoneId ?? Zone.Id; }
            set { _zoneId = value; }
        }

        public Zone Zone { get; set; }

        private int? _currencyId { get; set; }

        [XmlElement("id_currency")]
        public int CurrencyId
        {
            get { return _currencyId ?? Currency.Id; }
            set { _currencyId = value; }
        }

        public Currency Currency { get; set; }

        [XmlElement("contains_states")]
        public byte ContainsStates { get; set; }

        [XmlElement("need_identification_number")]
        public byte NeedIdentificationNumber { get; set; }

        [XmlElement("display_tax_label")]
        public byte DisplayTaxLabel { get; set; }

        [XmlArray("name")]
        [XmlArrayItem("language", typeof(LanguageTraduction))]
        public List<LanguageTraduction> Description { get; }
    }

    public class Zone : IIdentifiable
    {
        [XmlElement("id")]
        public override int Id { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }
    }

    public class Currency : IIdentifiable
    {
        [XmlElement("id")]
        public override int Id { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("iso_code")]
        public string IsoCode { get; set; }

        [XmlElement("conversion_rate")]
        public float ConversionRate { get; set; }

        [ContractInvariantMethod]
        protected void ObjectInvariant()
        {
            Contract.Invariant(this.IsoCode.Length == 3);
        }
    }
}
