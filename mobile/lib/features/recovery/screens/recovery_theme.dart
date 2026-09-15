import 'package:flutter/material.dart';

ThemeData recoveryTheme(BuildContext context) => Theme.of(context).copyWith(
      colorScheme: ColorScheme.fromSeed(seedColor: const Color(0xff087f76))
          .copyWith(surface: Colors.white),
      scaffoldBackgroundColor: const Color(0xfff7f8f5),
      cardTheme: CardTheme(
        color: Colors.white,
        surfaceTintColor: Colors.transparent,
        elevation: 0,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(6),
          side: const BorderSide(color: Color(0xffdce4e1)),
        ),
      ),
    );
