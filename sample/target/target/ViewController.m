//
//  ViewController.m
//  target
//
//  Created by wnxd on 2017/11/24.
//  Copyright © 2017年 wnxd. All rights reserved.
//

#import "ViewController.h"
#import "test.h"

@implementation ViewController

- (void)viewDidLoad {
    [super viewDidLoad];

    // Do any additional setup after loading the view.
}


- (void)setRepresentedObject:(id)representedObject {
    [super setRepresentedObject:representedObject];

    // Update the view, if already loaded.
}


- (IBAction)btAction:(id)sender {
    test* t = [[test alloc] init];
    [t text:@"hello world"];
}

@end
