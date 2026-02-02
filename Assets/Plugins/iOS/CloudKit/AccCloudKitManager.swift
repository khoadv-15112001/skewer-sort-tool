import Foundation
import CloudKit

@objc public class AccCloudKitManager: NSObject {
    private let privateDB = CKContainer.default().privateCloudDatabase
    private let recordID = CKRecord.ID(recordName: "FirebaseCredentialMain")
    private let userDefaultsKey = "backup_user_email"

    @objc public func saveEmail(_ email: String) {
        // 1) cache local for fast unity read
        UserDefaults.standard.set(email, forKey: userDefaultsKey)
        UserDefaults.standard.synchronize()

        // 2) push to CloudKit (private DB)
        let record = CKRecord(recordType: "FirebaseCredential", recordID: recordID)
        record["email"] = email as CKRecordValue

        privateDB.save(record) { saved, error in
            if let e = error {
                NSLog("[AccCloudKitManager] CloudKit save error: \(e.localizedDescription)")
            } else {
                NSLog("[AccCloudKitManager] saved email to CloudKit")
            }
        }
    }

    @objc public func getCachedEmail() -> String {
        return UserDefaults.standard.string(forKey: userDefaultsKey) ?? ""
    }

    @objc public func fetchFromCloudKitAndCache(completion: @escaping (String?) -> Void) {
        privateDB.fetch(withRecordID: recordID) { record, error in
            if let rec = record, let email = rec["email"] as? String {
                UserDefaults.standard.set(email, forKey: self.userDefaultsKey)
                UserDefaults.standard.synchronize()
                NSLog("[AccCloudKitManager] Fetched email from CloudKit and cached")
                completion(email)
            } else {
                if let e = error {
                    NSLog("[AccCloudKitManager] CloudKit fetch error: \(e.localizedDescription)")
                }
                completion(nil)
            }
        }
    }
}
