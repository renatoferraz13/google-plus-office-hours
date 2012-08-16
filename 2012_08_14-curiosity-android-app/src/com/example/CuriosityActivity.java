/*
 * Copyright 2012 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package com.example;

import android.app.Activity;
import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.os.Environment;
import android.provider.MediaStore;
import android.support.v4.app.ShareCompat;
import android.util.Log;
import android.view.View;
import java.io.File;
import java.util.Random;

public class CuriosityActivity extends Activity {

    private static final int IMAGE_REQUEST = 1;
    private static final String PIC_FILENAME = "mars.jpg";
    private static final String GOOGLE_APP_PACKAGE = "com.google.android.apps.plus";

    public void onCreate(Bundle savedInstanceState) {
        setContentView(R.layout.main);
        super.onCreate(savedInstanceState);
    }

    public void shareMSLImage(View view) {
        String[] images = getResources().getStringArray(R.array.mars_img_array);
        String[] comments = getResources().getStringArray(R.array.share_comment_array);
        Random r = new Random();
        int rand = r.nextInt(images.length);
        Intent shareIntent = ShareCompat.IntentBuilder.from(CuriosityActivity.this)
                .setText(comments[rand] + images[rand])
                .setType("text/plain")
                .getIntent()
                .setPackage(GOOGLE_APP_PACKAGE);

        startActivity(shareIntent);
    }

    public void sharePlusPage(View view) {
        Intent shareIntent = ShareCompat.IntentBuilder.from(CuriosityActivity.this)
                .setText(getString(R.string.share_plus_string) + getString(R.string.share_plus_page))
                .setType("text/plain")
                .getIntent()
                .setPackage(GOOGLE_APP_PACKAGE);

        startActivity(shareIntent);
    }

    public void takeAndSharePhoto(View view) {
        Intent intent = new Intent(android.provider.MediaStore.ACTION_IMAGE_CAPTURE);
        final File f = new File(Environment.getExternalStorageDirectory(), PIC_FILENAME);
        intent.putExtra(MediaStore.EXTRA_OUTPUT, Uri.fromFile(f));
        startActivityForResult(intent, IMAGE_REQUEST);
    }

    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        if (requestCode == IMAGE_REQUEST && resultCode == RESULT_OK) {
            try {
                final File tmpFile = new File(Environment.getExternalStorageDirectory(),
                        PIC_FILENAME);
                final String photoUri = MediaStore.Images.Media.insertImage(
                        getContentResolver(), tmpFile.getAbsolutePath(), null, null);

                String[] comments = getResources().getStringArray(R.array.share_comment_array);
                Random r = new Random();
                int rand = r.nextInt(comments.length);

                Intent shareIntent = ShareCompat.IntentBuilder.from(CuriosityActivity.this)
                        .setText(comments[rand])
                        .setType("image/jpeg")
                        .setStream(Uri.parse(photoUri))
                        .getIntent()
                        .setPackage(GOOGLE_APP_PACKAGE);

                startActivity(shareIntent);
                tmpFile.delete();

            } catch (Exception e) {
                Log.d("Curiosity", "Connection to Earth faltered. Oops! " + e.toString());
            }
        }
    }
}