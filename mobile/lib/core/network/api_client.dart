import 'package:dio/dio.dart';

import '../config/app_config.dart';

final Dio apiClient = Dio(
  BaseOptions(
    baseUrl: AppConfig.apiBaseUrl,
    connectTimeout: const Duration(seconds: 15),
    receiveTimeout: const Duration(seconds: 15),
  ),
);
