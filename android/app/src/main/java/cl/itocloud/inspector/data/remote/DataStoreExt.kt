package cl.itocloud.inspector.data.remote

import android.content.Context
import androidx.datastore.core.DataStore
import androidx.datastore.preferences.core.Preferences
import androidx.datastore.preferences.preferencesDataStore

/**
 * Extension property to get the auth DataStore instance from any Context.
 */
val Context.authDataStore: DataStore<Preferences> by preferencesDataStore(name = "auth_prefs")
