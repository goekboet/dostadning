#! bin/bash

function makefolders {
    tail -n +2 testdata.csv | cut -d';' -f1 | xargs mkdir
}