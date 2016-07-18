﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;

namespace PagarMe.Mpos
{	
	public class ApplicationEntry {
		public int PaymentMethod { get; set; }
		public string CardBrand { get; set; }

		public int AcquirerNumber { get; set; }
		public int RecordNumber { get; set; }
	}

	public class AcquirerEntry {
		public int Number { get; set; }
		public int CryptographyMethod { get; set; }
		public int KeyIndex { get; set; }

		public string SessionKey { get; set; }
		public string EmvTags { get; set; }
	}

	public class RiskManagementEntry {
		public int AcquirerNumber { get; set; }
		public int RecordNumber { get; set; }

		public bool MustRiskManagement { get; set; }
		public int FloorLimit { get; set; }
		public int BiasedRandomSelectionPercentage { get; set; }
		public int BiasedRandomSelectionThreshold { get; set; }
		public int BiasedRandomSelectionMaxPercentage { get; set; }
	}
	
	public class TMSStorage
	{
		private SQLiteConnection db;	

		public TMSStorage(string path, string encryptionKey) {
			db = new SQLiteConnection(path + "pagarme_mpos.sqlite");
			db.CreateTable<ApplicationEntry>();
			db.CreateTable<AcquirerEntry>();
			db.CreateTable<RiskManagementEntry>();
		}

		public void PurgeIndex() {
			db.DeleteAll<ApplicationEntry>();
			db.DeleteAll<AcquirerEntry>();
			db.DeleteAll<RiskManagementEntry>();
		}
		
		public void StoreAcquirerRow(int number, int cryptographyMethod, int keyIndex, byte[] sessionKey, int emvTagsLength, int[] emvTags) {
			int[] cleanEmvTags = new int[emvTagsLength];
			for (int i = 0; i < emvTagsLength; i++) {
				cleanEmvTags[i] = emvTags[i];
			}

			Console.WriteLine("[Storage] SessionKey = " + Encoding.ASCII.GetString(sessionKey, 0, 32));
			Console.WriteLine("[Storage] EmvTags = " + String.Join(",", cleanEmvTags));

			AcquirerEntry entry = new AcquirerEntry {
				Number = number,
				CryptographyMethod = cryptographyMethod,
				KeyIndex = keyIndex,
				SessionKey = Encoding.ASCII.GetString(sessionKey, 0, 32),
				EmvTags = String.Join(",", cleanEmvTags)
			};
			db.Insert(entry);
		}

		public void StoreRiskManagementRow(int acquirerNumber, int recordNumber, bool mustRiskManagement, int floorLimit, int brsPercentage, int brsThreshold, int brsMaxPercentage) {
			RiskManagementEntry entry = new RiskManagementEntry {
				AcquirerNumber = acquirerNumber,
				RecordNumber = recordNumber,
				MustRiskManagement = mustRiskManagement,
				FloorLimit = floorLimit,
				BiasedRandomSelectionPercentage = brsPercentage,
				BiasedRandomSelectionThreshold = brsThreshold,
				BiasedRandomSelectionMaxPercentage = brsMaxPercentage				
			};
			db.Insert(entry);			
		}

		public void StoreApplicationRow(int paymentMethod, string cardBrand, int acquirerNumber, int recordNumber) {
			ApplicationEntry entry = new ApplicationEntry {
				PaymentMethod = paymentMethod,
				CardBrand = cardBrand,
				AcquirerNumber = acquirerNumber,
				RecordNumber = recordNumber
			};
			db.Insert(entry);
		}		

		public AcquirerEntry[] GetAcquirerRows() {
			return db.Table<AcquirerEntry>().ToArray();
		}

		public RiskManagementEntry[] GetRiskManagementRows() {
			return db.Table<RiskManagementEntry>().ToArray();
		}

		public ApplicationEntry SelectApplication(string brand, int paymentMethod) {
			var query = db.Table<ApplicationEntry>().Where(e => (e.PaymentMethod == paymentMethod && e.CardBrand == brand));
			return query.FirstOrDefault();
		}
	}
}

