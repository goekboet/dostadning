#!bin/bash

add () {
    task add $1 project:dostadning
}

depends () {
    task $1 modify depends:$2
}

fin () {
    task $1 done
}