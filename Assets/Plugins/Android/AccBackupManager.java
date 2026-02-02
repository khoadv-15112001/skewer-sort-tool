package com.grill.sorting.food.match.puzzle;

import android.content.Context;
import android.content.SharedPreferences;

public class AccBackupManager {
    private static final String PREF_NAME = "acc_backup_storage";
    private static final String KEY_ACC_ID = "backup_user_email";

    public static void saveAccId(Context context, String accId) {
        if (context == null) {
            return;
        }

        SharedPreferences prefs = context.getSharedPreferences(PREF_NAME, Context.MODE_PRIVATE);
        prefs.edit().putString(KEY_ACC_ID, accId).apply();
    }

	public static String loadAccId(Context context) {
		if (context == null) {
			return "";
		}

		SharedPreferences prefs = context.getSharedPreferences(PREF_NAME, Context.MODE_PRIVATE);
		String accId = prefs.getString(KEY_ACC_ID, "");

		return accId;
	}

    public static void clearAccId(Context context) {
        if (context == null) {
            return;
        }

        SharedPreferences prefs = context.getSharedPreferences(PREF_NAME, Context.MODE_PRIVATE);
        prefs.edit().remove(KEY_ACC_ID).apply();
    }
}